; ModuleID = 'repl_module'
source_filename = "repl_module"

@str = private unnamed_addr constant [3 x i8] c"ha\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"_ho\00", align 1
@fmt_concat = private unnamed_addr constant [5 x i8] c"%s%s\00", align 1

define ptr @main_6() {
entry:
  %concat_buf = call ptr @malloc(i64 512)
  %sprintf_result = call i32 (ptr, ptr, ...) @sprintf(ptr %concat_buf, ptr @fmt_concat, ptr @str, ptr @str.1)
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 4, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %concat_buf, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare i32 @sprintf(ptr, ptr, ...)
