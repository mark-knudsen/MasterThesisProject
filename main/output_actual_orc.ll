; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr
@str = private unnamed_addr constant [11 x i8] c"delete.csv\00", align 1

define ptr @main_2() {
entry:
  %x_load = load ptr, ptr @x, align 8
  call void @ToCsvInternal(ptr %x_load, ptr @str)
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare void @ToCsvInternal(ptr, ptr)

declare noalias ptr @malloc(i64)
