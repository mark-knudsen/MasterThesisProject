; ModuleID = 'repl_module'
source_filename = "repl_module"

@df2 = external global ptr
@arr2 = external global ptr

define ptr @main_4() {
entry:
  %df2_load = load ptr, ptr @df2, align 8
  %arr2_load = load ptr, ptr @arr2, align 8
  %remove_len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr2_load, i32 0, i32 0
  %remove_data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr2_load, i32 0, i32 2
  %remove_len = load i64, ptr %remove_len_ptr, align 4
  %remove_data_ptr = load ptr, ptr %remove_data_ptr_ptr, align 8
  %df_rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df2_load, i32 0, i32 1
  %rows_header_ptr = load ptr, ptr %df_rows_field, align 8
  %src_len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_header_ptr, i32 0, i32 0
  %src_data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_header_ptr, i32 0, i32 2
  %src_data_ptr = load ptr, ptr %src_data_ptr_ptr, align 8
  %src_len = load i64, ptr %src_len_ptr, align 4
  br label %filter_start

filter_start:                                     ; preds = %entry
  %read_idx_ptr = alloca i64, align 8
  %write_idx_ptr = alloca i64, align 8
  store i64 0, ptr %read_idx_ptr, align 4
  store i64 0, ptr %write_idx_ptr, align 4
  br label %filter_loop

filter_loop:                                      ; preds = %filter_next, %filter_start
  %current_read_idx = load i64, ptr %read_idx_ptr, align 4
  %filter_cond = icmp slt i64 %current_read_idx, %src_len
  br i1 %filter_cond, label %filter_body, label %removerange_after

filter_body:                                      ; preds = %filter_loop
  br label %check_match_loop

check_match_loop:                                 ; preds = %next_match, %filter_body
  %match_i = phi i64 [ 0, %filter_body ], [ %next_match_i, %next_match ]
  %match_cond = icmp slt i64 %match_i, %remove_len
  br i1 %match_cond, label %check_match_body, label %perform_copy

check_match_body:                                 ; preds = %check_match_loop
  %rem_idx_ptr = getelementptr i64, ptr %remove_data_ptr, i64 %match_i
  %rem_idx_target = load i64, ptr %rem_idx_ptr, align 4
  %is_match = icmp eq i64 %current_read_idx, %rem_idx_target
  br i1 %is_match, label %filter_next, label %next_match

perform_copy:                                     ; preds = %check_match_loop
  %current_write_idx = load i64, ptr %write_idx_ptr, align 4
  %src_elem_gep = getelementptr ptr, ptr %src_data_ptr, i64 %current_read_idx
  %loaded_elem = load ptr, ptr %src_elem_gep, align 8
  %dst_elem_gep = getelementptr ptr, ptr %src_data_ptr, i64 %current_write_idx
  store ptr %loaded_elem, ptr %dst_elem_gep, align 8
  %updated_write_idx = add i64 %current_write_idx, 1
  store i64 %updated_write_idx, ptr %write_idx_ptr, align 4
  br label %filter_next

filter_next:                                      ; preds = %perform_copy, %check_match_body
  %next_read_idx = add i64 %current_read_idx, 1
  store i64 %next_read_idx, ptr %read_idx_ptr, align 4
  br label %filter_loop

removerange_after:                                ; preds = %filter_loop
  %total_written_elements = load i64, ptr %write_idx_ptr, align 4
  store i64 %total_written_elements, ptr %src_len_ptr, align 4
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %df2_load, ptr %data_ptr, align 8
  ret ptr %runtime_obj

next_match:                                       ; preds = %check_match_body
  %next_match_i = add i64 %match_i, 1
  br label %check_match_loop
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
