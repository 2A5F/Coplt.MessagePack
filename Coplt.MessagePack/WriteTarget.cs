using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Coplt.MessagePack;

public interface IWriteTarget
{
    public void Write(scoped ReadOnlySpan<byte> data);
    public void Write<T0>(T0 t0) where T0 : unmanaged;
    public void Write<T0, T1>(T0 t0, T1 t1) where T0 : unmanaged where T1 : unmanaged;
    public void Write<T0, T1, T2>(T0 t0, T1 t1, T2 t2) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged;
}

public interface IAsyncWriteTarget
{
    public ValueTask WriteAsync(ReadOnlyMemory<byte> data);
    public ValueTask WriteAsync(scoped ReadOnlySpan<byte> data);
    public ValueTask WriteAsync<T0>(T0 t0) where T0 : unmanaged;
    public ValueTask WriteAsync<T0, T1>(T0 t0, T1 t1) where T0 : unmanaged where T1 : unmanaged;
    public ValueTask WriteAsync<T0, T1, T2>(T0 t0, T1 t1, T2 t2) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged;
}

public readonly unsafe struct ListWriteTarget(List<byte> List) : IWriteTarget
{
    public readonly List<byte> List = List;

    public void Write(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return;
        if (data.Length == 1)
        {
            List.Add(data[0]);
        }
        var start = List.Count;
        CollectionsMarshal.SetCount(List, start + data.Length);
        var span = CollectionsMarshal.AsSpan(List)[start..];
        data.CopyTo(span);
    }

    public void Write<T0>(T0 t0) where T0 : unmanaged
    {
        if (sizeof(T0) == 1)
        {
            List.Add(Unsafe.BitCast<T0, byte>(t0));
        }
        var start = List.Count;
        CollectionsMarshal.SetCount(List, start + sizeof(T0));
        var span = CollectionsMarshal.AsSpan(List)[start..];
        Unsafe.As<byte, T0>(ref span[0]) = t0;
    }

    public void Write<T0, T1>(T0 t0, T1 t1) where T0 : unmanaged where T1 : unmanaged
    {
        var len = sizeof(T0) + sizeof(T1);
        var start = List.Count;
        CollectionsMarshal.SetCount(List, start + len);
        var span = CollectionsMarshal.AsSpan(List)[start..];
        Unsafe.As<byte, T0>(ref span[0]) = t0;
        Unsafe.As<byte, T1>(ref span[sizeof(T0)]) = t1;
    }

    public void Write<T0, T1, T2>(T0 t0, T1 t1, T2 t2) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        var len = sizeof(T0) + sizeof(T1) + sizeof(T2);
        var start = List.Count;
        CollectionsMarshal.SetCount(List, start + len);
        var span = CollectionsMarshal.AsSpan(List)[start..];
        Unsafe.As<byte, T0>(ref span[0]) = t0;
        Unsafe.As<byte, T1>(ref span[sizeof(T0)]) = t1;
        Unsafe.As<byte, T2>(ref span[sizeof(T0) + sizeof(T1)]) = t2;
    }
}

public unsafe ref struct SpanWriteTarget(Span<byte> Span) : IWriteTarget
{
    public Span<byte> Span { get; } = Span;
    private int m_len;

    public void Write(scoped ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return;
        if (m_len + data.Length > Span.Length) throw new OutOfCapacityException();
        data.CopyTo(Span[m_len..]);
        m_len += data.Length;
    }
    public void Write<T0>(T0 t0) where T0 : unmanaged
    {
        var len = sizeof(T0);
        if (m_len + len > Span.Length) throw new OutOfCapacityException();
        var span = Span[m_len..];
        Unsafe.As<byte, T0>(ref span[0]) = t0;
    }
    public void Write<T0, T1>(T0 t0, T1 t1) where T0 : unmanaged where T1 : unmanaged
    {
        var len = sizeof(T0) + sizeof(T1);
        if (m_len + len > Span.Length) throw new OutOfCapacityException();
        var span = Span[m_len..];
        Unsafe.As<byte, T0>(ref span[0]) = t0;
        Unsafe.As<byte, T1>(ref span[sizeof(T0)]) = t1;
    }
    public void Write<T0, T1, T2>(T0 t0, T1 t1, T2 t2) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        var len = sizeof(T0) + sizeof(T1) + sizeof(T2);
        if (m_len + len > Span.Length) throw new OutOfCapacityException();
        var span = Span[m_len..];
        Unsafe.As<byte, T0>(ref span[0]) = t0;
        Unsafe.As<byte, T1>(ref span[sizeof(T0)]) = t1;
        Unsafe.As<byte, T2>(ref span[sizeof(T0) + sizeof(T1)]) = t2;
    }
}

public readonly struct StreamWriteTarget(Stream Stream) : IWriteTarget
{
    public Stream Stream { get; } = Stream;

    public void Write(scoped ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return;
        Stream.Write(data);
    }
    public void Write<T0>(T0 t0) where T0 : unmanaged
    {
        Stream.Write(MemoryMarshal.AsBytes([t0]));
    }
    public void Write<T0, T1>(T0 t0, T1 t1) where T0 : unmanaged where T1 : unmanaged
    {
        Stream.Write(MemoryMarshal.AsBytes([t0]));
        Stream.Write(MemoryMarshal.AsBytes([t1]));
    }
    public void Write<T0, T1, T2>(T0 t0, T1 t1, T2 t2) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        Stream.Write(MemoryMarshal.AsBytes([t0]));
        Stream.Write(MemoryMarshal.AsBytes([t1]));
        Stream.Write(MemoryMarshal.AsBytes([t2]));
    }
}

public readonly struct AsyncStreamWriteTarget(Stream Stream, bool StreamOwner = false) : IDisposable, IAsyncDisposable, IAsyncWriteTarget
{
    #region Stream

    public readonly Stream Stream = Stream;

    #endregion

    #region Tmp Buffer

    private readonly byte[] m_tmp_buf = ArrayPool<byte>.Shared.Rent(1024);

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

    public ValueTask WriteAsync(scoped ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return ValueTask.CompletedTask;
        if (data.Length <= m_tmp_buf.Length)
        {
            data.CopyTo(m_tmp_buf);
            return Stream.WriteAsync(m_tmp_buf.AsMemory(0, data.Length));
        }

        var arr = ArrayPool<byte>.Shared.Rent(data.Length);
        try
        {
            data.CopyTo(arr);
        }
        catch
        {
            ArrayPool<byte>.Shared.Return(arr);
            throw;
        }
        return Next_Return(Stream, arr, data.Length);

        static async ValueTask Next_Return(Stream stream, byte[] arr, int len)
        {
            try
            {
                await stream.WriteAsync(arr.AsMemory(0, len));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(arr);
            }
        }
    }
    public ValueTask WriteAsync(ReadOnlyMemory<byte> data)
    {
        if (data.IsEmpty) return ValueTask.CompletedTask;
        return Stream.WriteAsync(data);
    }
    public async ValueTask WriteAsync<T0>(T0 t0) where T0 : unmanaged
    {
        var len = Unsafe.SizeOf<T0>();
        if (len <= m_tmp_buf.Length)
        {
            Unsafe.As<byte, T0>(ref m_tmp_buf[0]) = t0;
            await Stream.WriteAsync(m_tmp_buf.AsMemory(0, len));
        }
        else
        {
            var arr = ArrayPool<byte>.Shared.Rent(len);
            try
            {
                Unsafe.As<byte, T0>(ref arr[0]) = t0;
                await Stream.WriteAsync(arr.AsMemory(0, len));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(arr);
            }
        }
    }
    public async ValueTask WriteAsync<T0, T1>(T0 t0, T1 t1) where T0 : unmanaged where T1 : unmanaged
    {
        var len = Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>();
        if (len <= m_tmp_buf.Length)
        {
            Unsafe.As<byte, T0>(ref m_tmp_buf[0]) = t0;
            Unsafe.As<byte, T1>(ref m_tmp_buf[Unsafe.SizeOf<T0>()]) = t1;
            await Stream.WriteAsync(m_tmp_buf.AsMemory(0, len));
        }
        else
        {
            var arr = ArrayPool<byte>.Shared.Rent(len);
            try
            {
                Unsafe.As<byte, T0>(ref arr[0]) = t0;
                Unsafe.As<byte, T1>(ref arr[Unsafe.SizeOf<T0>()]) = t1;
                await Stream.WriteAsync(arr.AsMemory(0, len));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(arr);
            }
        }
    }
    public async ValueTask WriteAsync<T0, T1, T2>(T0 t0, T1 t1, T2 t2) where T0 : unmanaged where T1 : unmanaged where T2 : unmanaged
    {
        var len = Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
        if (len <= m_tmp_buf.Length)
        {
            Unsafe.As<byte, T0>(ref m_tmp_buf[0]) = t0;
            Unsafe.As<byte, T1>(ref m_tmp_buf[Unsafe.SizeOf<T0>()]) = t1;
            Unsafe.As<byte, T2>(ref m_tmp_buf[Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>()]) = t2;
            await Stream.WriteAsync(m_tmp_buf.AsMemory(0, len));
        }
        else
        {
            var arr = ArrayPool<byte>.Shared.Rent(len);
            try
            {
                Unsafe.As<byte, T0>(ref arr[0]) = t0;
                Unsafe.As<byte, T1>(ref arr[Unsafe.SizeOf<T0>()]) = t1;
                Unsafe.As<byte, T2>(ref arr[Unsafe.SizeOf<T0>() + Unsafe.SizeOf<T1>()]) = t2;
                await Stream.WriteAsync(arr.AsMemory(0, len));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(arr);
            }
        }
    }
}
