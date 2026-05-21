; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = global ptr null, align 8

define ptr @main_0() {
entry:
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 64)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 8, ptr %len_ptr, align 8
  store i64 8, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  %elem_ptr = getelementptr i64, ptr %arr_data_raw, i64 0
  store i64 10, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr i64, ptr %arr_data_raw, i64 1
  store i64 20, ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr i64, ptr %arr_data_raw, i64 2
  store i64 30, ptr %elem_ptr2, align 8
  %elem_ptr3 = getelementptr i64, ptr %arr_data_raw, i64 3
  store i64 40, ptr %elem_ptr3, align 8
  %elem_ptr4 = getelementptr i64, ptr %arr_data_raw, i64 4
  store i64 50, ptr %elem_ptr4, align 8
  %elem_ptr5 = getelementptr i64, ptr %arr_data_raw, i64 5
  store i64 60, ptr %elem_ptr5, align 8
  %elem_ptr6 = getelementptr i64, ptr %arr_data_raw, i64 6
  store i64 70, ptr %elem_ptr6, align 8
  %elem_ptr7 = getelementptr i64, ptr %arr_data_raw, i64 7
  store i64 80, ptr %elem_ptr7, align 8
  store ptr %arr_header, ptr @arr, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
