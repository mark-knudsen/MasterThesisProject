; ModuleID = 'repl_module'
source_filename = "repl_module"

define { i32, ptr } @main_1() {
entry:
  %int_mem = call ptr @malloc(i64 8)
  store i64 5, ptr %int_mem, align 8
  %with_data = insertvalue { i32, ptr } { i32 1, ptr undef }, ptr %int_mem, 1
  ret { i32, ptr } %with_data
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
