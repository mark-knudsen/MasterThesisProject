; ModuleID = 'repl_module'
source_filename = "repl_module"

@df = external global ptr
@x = external global i64
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_5() {
entry:
  %df_load = load ptr, ptr @df, align 8
  %columns_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_load, i32 0, i32 0
  %columns_ptr = load ptr, ptr %columns_ptr_field, align 8
  %x_load = load i64, ptr @x, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %columns_ptr, i32 0, i32 0
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %columns_ptr, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_field_ptr, align 8
  %array_len = load i64, ptr %len_field_ptr, align 8
  %index_is_neg = icmp slt i64 %x_load, 0
  %index_rel = add i64 %x_load, %array_len
  %resolved_index = select i1 %index_is_neg, i64 %index_rel, i64 %x_load
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
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 4, ptr %tag_ptr, align 8
  %data_ptr1 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %loaded_val, ptr %data_ptr1, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
