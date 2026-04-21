; ModuleID = 'repl_module'
source_filename = "repl_module"

define ptr @main_0() {
entry:
  %sqrttmp = call double @llvm.sqrt.f64(double 1.000000e+02)
  %value_mem = call ptr @malloc(i64 8)
  store double %sqrttmp, ptr %value_mem, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 2, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

; Function Attrs: nocallback nofree nosync nounwind speculatable willreturn memory(none)
declare double @llvm.sqrt.f64(double) #0

declare ptr @malloc(i64)

attributes #0 = { nocallback nofree nosync nounwind speculatable willreturn memory(none) }
