; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_2() {
entry:
  %arr_load = load ptr, ptr @arr, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_load, i32 0, i32 0
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_load, i32 0, i32 2
  %array_len = load i64, ptr %len_field_ptr, align 8
  %data_ptr = load ptr, ptr %data_field_ptr, align 8
  %0 = icmp sge i64 0, %array_len
  %is_invalid = or i1 false, %0
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

bounds.fail:                                      ; preds = %entry
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %entry
  %elem_ptr = getelementptr i64, ptr %data_ptr, i64 0
  %loaded_val = load i64, ptr %elem_ptr, align 8
  %value_mem = call ptr @malloc(i64 8)
  store i64 %loaded_val, ptr %value_mem, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 1, ptr %tag_ptr, align 8
  %data_ptr1 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr1, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
