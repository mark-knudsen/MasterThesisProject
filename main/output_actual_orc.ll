; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = global i64 0

define ptr @main_1() {
entry:
  store i64 2, ptr @x, align 8
  %inc_load = load i64, ptr @x, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @x, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
