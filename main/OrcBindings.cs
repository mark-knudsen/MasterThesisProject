using System.Runtime.InteropServices;
internal static class OrcBindings
{
    private const string LibLLVM = "libLLVM-20.so"; // linux
    //private const string LibLLVM = "libLLVM.dll"; // windows

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcCreateLLJIT(
        out IntPtr Result,
        IntPtr Options);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcLLJITAddLLVMIRModule(
        IntPtr J,
        IntPtr JD,
        IntPtr TSM);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcLLJITLookup(
        IntPtr J,
        out ulong Result,
        [MarshalAs(UnmanagedType.LPStr)] string Name);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcLLJITGetMainJITDylib(
        IntPtr J);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcCreateNewThreadSafeContext();

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcCreateNewThreadSafeModule(IntPtr Module, IntPtr Context);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcCreateDynamicLibrarySearchGeneratorForProcess(
    out IntPtr Result,
    IntPtr GlobalPrefix);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMOrcJITDylibAddGenerator(
        IntPtr JD,
        IntPtr Generator);

    [DllImport(LibLLVM)]
    public static extern IntPtr LLVMGetErrorMessage(IntPtr Err);

    [DllImport(LibLLVM)]
    public static extern void LLVMDisposeErrorMessage(IntPtr ErrMsg);

    [DllImport(LibLLVM)]
    public static extern void LLVMConsumeError(IntPtr Err);
}