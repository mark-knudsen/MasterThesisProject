; ModuleID = 'repl_module'
source_filename = "repl_module"

define { i32, ptr } @main_0() {
entry:
  %num_mem = call ptr @malloc(i64 8)
  store double 4.000000e+00, ptr %num_mem, align 8
  %with_data = insertvalue { i32, ptr } { i32 1, ptr undef }, ptr %num_mem, 1
  ret { i32, ptr } %with_data
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
