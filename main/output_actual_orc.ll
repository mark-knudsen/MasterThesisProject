; ModuleID = 'repl_module'
source_filename = "repl_module"

@df = external global ptr

define ptr @main_2() {
entry:
  %df_load = load ptr, ptr @df, align 8
  %cols_ptr_ptr = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_load, i32 0, i32 0
  %rows_ptr_ptr = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_load, i32 0, i32 1
  %types_ptr_ptr = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_load, i32 0, i32 2
  %orig_cols = load ptr, ptr %cols_ptr_ptr, align 8
  %orig_rows = load ptr, ptr %rows_ptr_ptr, align 8
  %orig_types = load ptr, ptr %types_ptr_ptr, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %orig_rows, i32 0, i32 0
  %source_len = load i64, ptr %len_ptr, align 4
  %0 = icmp sgt i64 0, %source_len
  %1 = select i1 %0, i64 %source_len, i64 0
  %start_final = select i1 false, i64 0, i64 %1
  %2 = icmp sgt i64 10, i64 %source_len
  %3 = icmp slt i64 10, i64 %start_final
  %4 = select i1 %3, i64 %start_final, i64 10
  %end_final = select i1 %2, i64 %source_len, i64 %4
  %new_len = sub i64 %end_final, %start_final
  %arr_header = call ptr @malloc(i64 24)
  %data_size = mul i64 %new_len, i64 8
  %arr_data = call ptr @malloc(i64 %data_size)
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 %new_len, ptr %5, align 4
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 %new_len, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %7, align 8
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  %arr_data1 = load ptr, ptr %8, align 8
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %orig_rows, i32 0, i32 2
  %arr_data2 = load ptr, ptr %9, align 8
  %src_typed = bitcast ptr %arr_data2 to ptr
  %dest_typed = bitcast ptr %arr_data1 to ptr
  %offset_src_ptr = getelementptr ptr, ptr %src_typed, i64 %start_final
  %index_ptr = alloca i64, align 8
  store i64 0, ptr %index_ptr, align 4
  br label %loop_cond

loop_cond:                                        ; preds = %loop_body, %entry
  %index = load i64, ptr %index_ptr, align 4
  %cond = icmp slt i64 %index, i64 %new_len
  br i1 %cond, label %loop_body, label %loop_end

loop_body:                                        ; preds = %loop_cond
  %el_ptr = getelementptr ptr, ptr %offset_src_ptr, i64 %index
  %el = load ptr, ptr %el_ptr, align 8
  %dest_ptr = getelementptr ptr, ptr %dest_typed, i64 %index
  store ptr %el, ptr %dest_ptr, align 8
  %next_index = add i64 %index, 1
  store i64 %next_index, ptr %index_ptr, align 4
  br label %loop_cond

loop_end:                                         ; preds = %loop_cond
  %new_df_header = call ptr @malloc(i64 24)
  %new_cols_ptr = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %new_df_header, i32 0, i32 0
  %new_rows_ptr = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %new_df_header, i32 0, i32 1
  %new_types_ptr = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %new_df_header, i32 0, i32 2
  store ptr %orig_cols, ptr %new_cols_ptr, align 8
  store ptr %arr_header, ptr %new_rows_ptr, align 8
  store ptr %orig_types, ptr %new_types_ptr, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %new_df_header, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
