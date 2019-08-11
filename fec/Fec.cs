using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public class Fec
{
    #region reed solomon

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct ReedSolomonData
    {
        public byte** arrays;
        public int* lengths;
        public int cnt;
    };

    [DllImport(@"reedsolomon.dll", EntryPoint = "ReedSolomon_Init", CallingConvention = CallingConvention.StdCall)]
    private static extern void ReedSolomon_Init(int dataShards, int parityShards);

    [DllImport(@"reedsolomon.dll", EntryPoint = "ReedSolomon_Encode", CallingConvention = CallingConvention.StdCall)]
    private static extern void ReedSolomon_Encode(ref IntPtr data);

    [DllImport(@"reedsolomon.dll", EntryPoint = "ReedSolomon_Reconstruct", CallingConvention = CallingConvention.StdCall)]
    private static extern void ReedSolomon_Reconstruct(ref IntPtr data);

    [DllImport(@"reedsolomon.dll", EntryPoint = "ReedSolomon_Create", CallingConvention = CallingConvention.StdCall)]
    private static extern void ReedSolomon_Create([MarshalAs(UnmanagedType.LPArray)]int[] arraySizes, int cnt, ref IntPtr data);

    [DllImport(@"reedsolomon.dll", EntryPoint = "ReedSolomon_Release", CallingConvention = CallingConvention.StdCall)]
    private static extern void ReedSolomon_Release(ref IntPtr data);

    private static void _Init(int dataShards, int parityShards)
    {
        ReedSolomon_Init(dataShards, parityShards);
    }

    private static void _Encode(ref List<List<byte>> datas)
    {
        IntPtr rs = Wrapper(datas);
        ReedSolomon_Encode(ref rs);
        datas = Unwrapper(rs);
    }

    private static void _Reconstruct(ref List<List<byte>> datas)
    {
        IntPtr rs = Wrapper(datas);
        ReedSolomon_Reconstruct(ref rs);
        datas = Unwrapper(rs);
    }

    private static List<List<byte>> Unwrapper(IntPtr ptr)
    {
        List<List<byte>> packs = new List<List<byte>>();

        unsafe
        {
            try
            {
                ReedSolomonData* data = (ReedSolomonData*)ptr;

                for (int i = 0; i < data->cnt; i++)
                {
                    byte* src = data->arrays[i];
                    int cnt = data->lengths[i];
                    byte[] dist = new byte[cnt];
                    Marshal.Copy((IntPtr)src, dist, 0, cnt);

                    List<byte> pack = new List<byte>(dist);
                    packs.Add(pack);
                }
            }
            finally
            {
                ReedSolomon_Release(ref ptr);
            }
        }

        return packs;
    }

    private static IntPtr Wrapper(List<List<byte>> packs)
    {
        int cnt = packs.Count;
        int[] arraySizes = new int[cnt];
        for (int i = 0; i < cnt; i++)
        {
            arraySizes[i] = packs[i].Count;
        }

        IntPtr ptr = IntPtr.Zero;

        try
        {
            unsafe
            {
                ReedSolomon_Create(arraySizes, cnt, ref ptr);

                ReedSolomonData* data = (ReedSolomonData*)ptr;

                for (int i = 0; i < cnt; i++)
                {
                    byte[] pack = packs[i].ToArray();
                    byte* dist = data->arrays[i];
                    Marshal.Copy(pack, 0, (IntPtr)dist, pack.Length);
                }
            }
        }
        catch (Exception)
        {
            throw;
        }

        return ptr;
    }

    #endregion reed solomon
}