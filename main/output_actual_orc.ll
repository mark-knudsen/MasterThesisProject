; ModuleID = 'repl_module'
source_filename = "repl_module"

@df2 = external global ptr
@str = private unnamed_addr constant [15 x i8] c"CSV/NEwNEW.csv\00", align 1

define ptr @main_3() {
entry:
  %df2_load = load ptr, ptr @df2, align 8
  call void @ToCsvInternal(ptr %df2_load, ptr @str)
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare void @ToCsvInternal(ptr, ptr)

declare noalias ptr @malloc(i64)
