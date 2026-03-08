; ModuleID = 'repl_module'
source_filename = "repl_module"

@str = private unnamed_addr constant [3 x i8] c"df\00", align 1
@x = global ptr null

define ptr @main_0() {
entry:
  store ptr @str, ptr @x, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i32 0, ptr %tag_ptr, align 4
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr @str, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
