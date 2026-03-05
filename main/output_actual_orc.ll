; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global double

define { i32, ptr } @main_7() {
entry:
  store double 2.200000e+00, ptr @x, align 8
  %x = load double, ptr @x, align 8
  %faddtmp = fadd double %x, 2.000000e+00
  %num_mem = call ptr @malloc(i64 8)
  store double %faddtmp, ptr %num_mem, align 8
  %with_data = insertvalue { i32, ptr } { i32 2, ptr undef }, ptr %num_mem, 1
  ret { i32, ptr } %with_data
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
