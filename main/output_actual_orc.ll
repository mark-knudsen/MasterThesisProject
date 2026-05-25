; ModuleID = 'repl_module'
source_filename = "repl_module"

@df3 = external global ptr

define ptr @main_1() {
entry:
  %df3_load = load ptr, ptr @df3, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %df3_load, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
