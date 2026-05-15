; ModuleID = 'repl_module'
source_filename = "repl_module"

@df2 = external global ptr

define ptr @main_2() {
entry:
  %df2_load = load ptr, ptr @df2, align 8
  %columns_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df2_load, i32 0, i32 0
  %columns_ptr = load ptr, ptr %columns_ptr_field, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 5, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %columns_ptr, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
