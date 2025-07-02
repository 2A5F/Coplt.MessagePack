using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Coplt.MessagePack;

public interface IReadSource : IDisposable
{
    public void Read(int length);
    public void Read<T0>() where T0 : unmanaged;
    public void Read<T0, T1>() where T0 : unmanaged where T1 : unmanaged;
    public void Read<T0, T1, T2>() where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged;

    public ReadOnlySpan<byte> Peek(int length);
    public T0? Peek<T0>() where T0 : unmanaged;
    public (T0, T1)? Peek<T0, T1>() where T0 : unmanaged where T1 : unmanaged;
    public (T0, T1, T2)? Peek<T0, T1, T2>() where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged;
}

public interface IAsyncReadSource : IDisposable, IAsyncDisposable
{
    public ValueTask ReadAsync(int length);
    public ValueTask ReadAsync<T0>() where T0 : unmanaged;
    public ValueTask ReadAsync<T0, T1>() where T0 : unmanaged where T1 : unmanaged;
    public ValueTask ReadAsync<T0, T1, T2>() where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged;

    public ValueTask<ReadOnlyMemory<byte>> PeekAsync(int length);
    public ValueTask<T0?> PeekAsync<T0>() where T0 : unmanaged;
    public ValueTask<(T0, T1)?> PeekAsync<T0, T1>() where T0 : unmanaged where T1 : unmanaged;
    public ValueTask<(T0, T1, T2)?> PeekAsync<T0, T1, T2>() where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged;
}

public unsafe ref struct SpanReadSource(ReadOnlySpan<byte> Span) : IReadSource
{
    public readonly ReadOnlySpan<byte> Span = Span;
    private int m_offset = 0;

    public SpanReadSource(List<byte> List) : this(CollectionsMarshal.AsSpan(List)) { }

    public void Read(int length)
    {
        if (m_offset + length > Span.Length) throw new OutOfCapacityException();
        m_offset += length;
    }

    public void Read<T0>() where T0 : unmanaged => Read(sizeof(T0));
    public void Read<T0, T1>() where T0 : unmanaged where T1 : unmanaged => Read(sizeof(T0) + sizeof(T1));
    public void Read<T0, T1, T2>() where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged => Read(sizeof(T0) + sizeof(T1) + sizeof(T2));

    public ReadOnlySpan<byte> Peek(int length)
    {
        var span = Span[m_offset..];
        return span[..Math.Min(span.Length, length)];
    }
    public T0? Peek<T0>() where T0 : unmanaged
    {
        var span = Peek(sizeof(T0));
        if (span.Length < sizeof(T0)) return null;
        return Unsafe.As<byte, T0>(ref Unsafe.AsRef(in span[0]));
    }
    public (T0, T1)? Peek<T0, T1>() where T0 : unmanaged where T1 : unmanaged
    {
        var span = Peek(sizeof(T0) + sizeof(T1));
        if (span.Length < sizeof(T0) + sizeof(T1)) return null;
        var t0 = Unsafe.As<byte, T0>(ref Unsafe.AsRef(in span[0]));
        var t1 = Unsafe.As<byte, T1>(ref Unsafe.AsRef(in span[sizeof(T0)]));
        return (t0, t1);
    }
    public (T0, T1, T2)? Peek<T0, T1, T2>() where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        var span = Peek(sizeof(T0) + sizeof(T1) + sizeof(T2));
        if (span.Length < sizeof(T0) + sizeof(T1) + sizeof(T2)) return null;
        var t0 = Unsafe.As<byte, T0>(ref Unsafe.AsRef(in span[0]));
        var t1 = Unsafe.As<byte, T1>(ref Unsafe.AsRef(in span[sizeof(T0)]));
        var t2 = Unsafe.As<byte, T2>(ref Unsafe.AsRef(in span[sizeof(T0) + sizeof(T1)]));
        return (t0, t1, t2);
    }

    public void Dispose() { }
}

public struct StreamReadSource(Stream Stream, bool StreamOwner = false) : IReadSource
{
    #region Stream

    public readonly Stream Stream = Stream;

    #endregion

    #region Tmp Buffer

    private byte[] m_tmp_buf = ArrayPool<byte>.Shared.Rent(1024);
    private int m_offset = 0;
    private int m_len = 0;

    #endregion

    #region Dispose

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(m_tmp_buf);
        if (StreamOwner) Stream.Dispose();
    }

    #endregion

    #region EnsureCapacity

    private void EnsureCapacity(int more_length)
    {
        var old_buffer = m_tmp_buf;
        if (m_offset + m_len + more_length < old_buffer.Length) return;
        if (m_offset != 0 && m_len + more_length < old_buffer.Length)
        {
            old_buffer.AsSpan(m_offset, m_len).CopyTo(old_buffer.AsSpan(0, m_len));
            m_offset = 0;
            return;
        }
        var new_buffer = ArrayPool<byte>.Shared.Rent(more_length);
        old_buffer.AsSpan(m_offset, m_len).CopyTo(new_buffer);
        m_tmp_buf = new_buffer;
        m_offset = 0;
        ArrayPool<byte>.Shared.Return(old_buffer);
    }

    #endregion

    public void Read(int length)
    {
        if (length > m_len) throw new OutOfCapacityException();
        m_offset += length;
        m_len -= length;
    }
    public void Read<T0>() where T0 : unmanaged
        => Read(Unsafe.SizeOf<T0>());
    public void Read<T0, T1>() where T0 : unmanaged where T1 : unmanaged
        => Read(Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>());
    public void Read<T0, T1, T2>() where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
        => Read(Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>());
    public ReadOnlySpan<byte> Peek(int length)
    {
        if (length <= m_len) return m_tmp_buf.AsSpan(m_offset, length);
        var len = length - m_len;
        EnsureCapacity(len);

        for (;;)
        {
            var count = Stream.Read(m_tmp_buf.AsSpan(m_offset + m_len, len));
            m_len += count;
            len -= count;
            if (count == 0 || len == 0) return m_tmp_buf.AsSpan(m_offset, m_len);
        }
    }
    public T0? Peek<T0>() where T0 : unmanaged
    {
        var span = Peek(Unsafe.SizeOf<T0>());
        if (span.Length < Unsafe.SizeOf<T0>()) return null;
        return Unsafe.As<byte, T0>(ref Unsafe.AsRef(in span[0]));
    }
    public (T0, T1)? Peek<T0, T1>() where T0 : unmanaged where T1 : unmanaged
    {
        var span = Peek(Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>());
        if (span.Length < Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>()) return null;
        var t0 = Unsafe.As<byte, T0>(ref Unsafe.AsRef(in span[0]));
        var t1 = Unsafe.As<byte, T1>(ref Unsafe.AsRef(in span[Unsafe.SizeOf<T0>()]));
        return (t0, t1);
    }
    public (T0, T1, T2)? Peek<T0, T1, T2>() where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        var span = Peek(Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>());
        if (span.Length < Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()) return null;
        var t0 = Unsafe.As<byte, T0>(ref Unsafe.AsRef(in span[0]));
        var t1 = Unsafe.As<byte, T1>(ref Unsafe.AsRef(in span[Unsafe.SizeOf<T0>()]));
        var t2 = Unsafe.As<byte, T2>(ref Unsafe.AsRef(in span[Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>()]));
        return (t0, t1, t2);
    }
}

public struct AsyncStreamReadSource(Stream Stream, bool StreamOwner = false) : IAsyncReadSource
{
    #region Stream

    public readonly Stream Stream = Stream;

    #endregion

    #region Tmp Buffer

    private byte[] m_tmp_buf = ArrayPool<byte>.Shared.Rent(1024);
    private int m_offset = 0;
    private int m_len = 0;

    #endregion

    #region Dispose

    public void Dispose()
    {
        ArrayPool<byte>.Shared.Return(m_tmp_buf);
        if (StreamOwner) Stream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        ArrayPool<byte>.Shared.Return(m_tmp_buf);
        if (StreamOwner) await Stream.DisposeAsync();
    }

    #endregion

    #region EnsureCapacity

    private void EnsureCapacity(int more_length)
    {
        var old_buffer = m_tmp_buf;
        if (m_offset + m_len + more_length < old_buffer.Length) return;
        if (m_offset != 0 && m_len + more_length < old_buffer.Length)
        {
            old_buffer.AsSpan(m_offset, m_len).CopyTo(old_buffer.AsSpan(0, m_len));
            m_offset = 0;
            return;
        }
        var new_buffer = ArrayPool<byte>.Shared.Rent(more_length);
        old_buffer.AsSpan(m_offset, m_len).CopyTo(new_buffer);
        m_tmp_buf = new_buffer;
        m_offset = 0;
        ArrayPool<byte>.Shared.Return(old_buffer);
    }

    #endregion

    public ValueTask ReadAsync(int length)
    {
        if (length > m_len) throw new OutOfCapacityException();
        m_offset += length;
        m_len -= length;
        return ValueTask.CompletedTask;
    }
    public ValueTask ReadAsync<T0>() where T0 : unmanaged
        => ReadAsync(Unsafe.SizeOf<T0>());
    public ValueTask ReadAsync<T0, T1>() where T0 : unmanaged where T1 : unmanaged
        => ReadAsync(Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>());
    public ValueTask ReadAsync<T0, T1, T2>() where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
        => ReadAsync(Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>());
    public async ValueTask<ReadOnlyMemory<byte>> PeekAsync(int length)
    {
        if (length <= m_len) return m_tmp_buf.AsMemory(m_offset, length);
        var len = length - m_len;
        EnsureCapacity(len);

        for (;;)
        {
            var count = await Stream.ReadAsync(m_tmp_buf.AsMemory(m_offset + m_len, len));
            m_len += count;
            len -= count;
            if (count == 0 || len == 0) return m_tmp_buf.AsMemory(m_offset, m_len);
        }
    }
    public async ValueTask<T0?> PeekAsync<T0>() where T0 : unmanaged
    {
        var memory = await PeekAsync(Unsafe.SizeOf<T0>());
        if (memory.Length < Unsafe.SizeOf<T0>()) return null;
        return Unsafe.As<byte, T0>(ref Unsafe.AsRef(in memory.Span[0]));
    }
    public async ValueTask<(T0, T1)?> PeekAsync<T0, T1>() where T0 : unmanaged where T1 : unmanaged
    {
        var memory = await PeekAsync(Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>());
        if (memory.Length < Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>()) return null;
        var t0 = Unsafe.As<byte, T0>(ref Unsafe.AsRef(in memory.Span[0]));
        var t1 = Unsafe.As<byte, T1>(ref Unsafe.AsRef(in memory.Span[Unsafe.SizeOf<T0>()]));
        return (t0, t1);
    }
    public async ValueTask<(T0, T1, T2)?> PeekAsync<T0, T1, T2>() where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        var span = await PeekAsync(Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>());
        if (span.Length < Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()) return null;
        var t0 = Unsafe.As<byte, T0>(ref Unsafe.AsRef(in span.Span[0]));
        var t1 = Unsafe.As<byte, T1>(ref Unsafe.AsRef(in span.Span[Unsafe.SizeOf<T0>()]));
        var t2 = Unsafe.As<byte, T2>(ref Unsafe.AsRef(in span.Span[Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>()]));
        return (t0, t1, t2);
    }
}
