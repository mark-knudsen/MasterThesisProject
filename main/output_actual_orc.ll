; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_21() {
entry:
  %arr_load = load ptr, ptr @arr, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_load, i32 0, i32 0
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_load, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_field_ptr, align 8
  %array_len = load i64, ptr %len_field_ptr, align 8
  %index_rel = add i64 0, %array_len
  %resolved_index = select i1 false, i64 %index_rel, i64 0
  %is_neg = icmp slt i64 %resolved_index, 0
  %is_too_big = icmp sge i64 %resolved_index, %array_len
  %is_invalid = or i1 %is_neg, %is_too_big
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

bounds.fail:                                      ; preds = %entry
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %entry
  %elem_ptr = getelementptr ptr, ptr %data_ptr, i64 %resolved_index
  %loaded_val = load ptr, ptr %elem_ptr, align 8
  %len_field_ptr1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %loaded_val, i32 0, i32 0
  %data_field_ptr2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %loaded_val, i32 0, i32 2
  %data_ptr3 = load ptr, ptr %data_field_ptr2, align 8
  %array_len4 = load i64, ptr %len_field_ptr1, align 8
  %index_rel5 = add i64 0, %array_len4
  %resolved_index6 = select i1 false, i64 %index_rel5, i64 0
  %is_neg7 = icmp slt i64 %resolved_index6, 0
  %is_too_big8 = icmp sge i64 %resolved_index6, %array_len4
  %is_invalid9 = or i1 %is_neg7, %is_too_big8
  br i1 %is_invalid9, label %bounds.fail10, label %bounds.ok11

bounds.fail10:                                    ; preds = %bounds.ok
  %print_err12 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok11:                                      ; preds = %bounds.ok
  %elem_ptr13 = getelementptr ptr, ptr %data_ptr3, i64 %resolved_index6
  %loaded_val14 = load ptr, ptr %elem_ptr13, align 8
  %len_field_ptr15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %loaded_val14, i32 0, i32 0
  %data_field_ptr16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %loaded_val14, i32 0, i32 2
  %data_ptr17 = load ptr, ptr %data_field_ptr16, align 8
  %array_len18 = load i64, ptr %len_field_ptr15, align 8
  %index_rel19 = add i64 0, %array_len18
  %resolved_index20 = select i1 false, i64 %index_rel19, i64 0
  %is_neg21 = icmp slt i64 %resolved_index20, 0
  %is_too_big22 = icmp sge i64 %resolved_index20, %array_len18
  %is_invalid23 = or i1 %is_neg21, %is_too_big22
  br i1 %is_invalid23, label %bounds.fail24, label %bounds.ok25

bounds.fail24:                                    ; preds = %bounds.ok11
  %print_err26 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok25:                                      ; preds = %bounds.ok11
  %elem_ptr27 = getelementptr i64, ptr %data_ptr17, i64 %resolved_index20
  %loaded_val28 = load i64, ptr %elem_ptr27, align 8
  %value_mem = call ptr @malloc(i64 8)
  store i64 %loaded_val28, ptr %value_mem, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 1, ptr %tag_ptr, align 8
  %data_ptr29 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr29, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
