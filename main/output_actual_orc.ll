; ModuleID = 'repl_module'
source_filename = "repl_module"

define ptr @main_12() {
entry:
  %rand = call i64 @rand()
  %rand_fp = sitofp i64 %rand to double
  %norm = fdiv double %rand_fp, 3.276700e+04
  %scaled = fmul double %norm, 9.000000e+00
  %rand_float = fadd double %scaled, 1.000000e+00
  %value_mem = call ptr @malloc(i64 8)
  %value_cast = bitcast ptr %value_mem to ptr
  store double %rand_float, ptr %value_cast, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 2, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare i64 @rand()

declare ptr @malloc(i64)
