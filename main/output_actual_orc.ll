; ModuleID = 'repl_module'
source_filename = "repl_module"

@fmt_float_raw = private unnamed_addr constant [4 x i8] c"%f\0A\00", align 1

define ptr @main_6() {
entry:
  %logtmp = call double @llvm.log.f64(double 1.000000e+00)
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_float_raw, double %logtmp)
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

; Function Attrs: nocallback nofree nosync nounwind speculatable willreturn memory(none)
declare double @llvm.log.f64(double) #0

declare ptr @malloc(i64)

attributes #0 = { nocallback nofree nosync nounwind speculatable willreturn memory(none) }
