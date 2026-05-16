; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_4() {
entry:
  %arr_load = load ptr, ptr @arr, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_load, i32 0, i32 0
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_load, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_field_ptr, align 8
  %array_len = load i64, ptr %len_field_ptr, align 8
  %index_rel = add i64 -3, %array_len
  %resolved_index = select i1 true, i64 %index_rel, i64 -3
  %is_neg = icmp slt i64 %resolved_index, 0
  %is_too_big = icmp sge i64 %resolved_index, %array_len
  %is_invalid = or i1 %is_neg, %is_too_big
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

bounds.fail:                                      ; preds = %entry
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %entry
  %elem_ptr = getelementptr i64, ptr %data_ptr, i64 %resolved_index
  %loaded_val = load i64, ptr %elem_ptr, align 8
  %value_mem = call ptr @malloc(i64 8)
  store i64 %loaded_val, ptr %value_mem, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 1, ptr %tag_ptr, align 8
  %data_ptr1 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr1, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
