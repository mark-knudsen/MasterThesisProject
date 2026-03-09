; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr

define ptr @main_3() {
entry:
  %arr_load = load ptr, ptr @arr, align 8
  %elem_ptr = getelementptr i64, ptr %arr_load, i64 3
  %raw_val = load i64, ptr %elem_ptr, align 8
  %int_mem = call ptr @malloc(i64 8)
  store i64 %raw_val, ptr %int_mem, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 1, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %int_mem, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
