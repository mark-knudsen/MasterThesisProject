; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = global ptr null

define ptr @main_0() {
entry:
  %arr_ptr = call ptr @malloc(i64 32)
  %len_ptr = getelementptr i64, ptr %arr_ptr, i32 0
  store i64 3, ptr %len_ptr, align 4
  %idx_0 = getelementptr i64, ptr %arr_ptr, i32 1
  store i64 7, ptr %idx_0, align 8
  %idx_1 = getelementptr i64, ptr %arr_ptr, i32 2
  store i64 8, ptr %idx_1, align 8
  %idx_2 = getelementptr i64, ptr %arr_ptr, i32 3
  store i64 9, ptr %idx_2, align 8
  store ptr %arr_ptr, ptr @x, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 0, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i32, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
