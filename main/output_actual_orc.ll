; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global i64

define ptr @main_7() {
entry:
  %x = load i64, ptr @x, align 4
  %int_mem = call ptr @malloc(i64 8)
  store i64 %x, ptr %int_mem, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 1, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %int_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
