; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }

@df2 = external global ptr
@"$src_rows" = external global ptr, align 8
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@__map_src = external global ptr, align 8
@__map_result = external global ptr, align 8
@__map_i = external global i64, align 8
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.3 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_2() {
entry:
  %df2_load = load ptr, ptr @df2, align 8
  %df_field_extract = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df2_load, i32 0, i32 1
  %arr_header_ptr = load ptr, ptr %df_field_extract, align 8
  store ptr %arr_header_ptr, ptr @"$src_rows", align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 32)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 2, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 4, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %"$src_rows_load" = load ptr, ptr @"$src_rows", align 8
  %src_len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %"$src_rows_load", i32 0, i32 0
  %src_data_ptr_field = getelementptr inbounds nuw { i64, i64, ptr }, ptr %"$src_rows_load", i32 0, i32 2
  %length = load i64, ptr %src_len_ptr, align 4
  %src_data_ptr = load ptr, ptr %src_data_ptr_field, align 8
  %new_header = call ptr @malloc(i64 24)
  %byte_count = mul i64 %length, 8
  %3 = icmp eq i64 %length, 0
  %4 = select i1 %3, i64 32, i64 %byte_count
  %new_data = call ptr @malloc(i64 %4)
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header, i32 0, i32 0
  store i64 %length, ptr %5, align 4
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header, i32 0, i32 1
  store i64 %length, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header, i32 0, i32 2
  store ptr %new_data, ptr %7, align 8
  %8 = icmp ugt i64 %length, 0
  br i1 %8, label %copy.do, label %copy.end

copy.do:                                          ; preds = %entry
  %9 = bitcast ptr %new_data to ptr
  %index_ptr = alloca i64, align 8
  store i64 0, ptr %index_ptr, align 4
  br label %array.copy.loop

copy.end:                                         ; preds = %array.copy.exit, %entry
  store ptr %new_header, ptr @__map_src, align 8
  %__map_src_load = load ptr, ptr @__map_src, align 8
  %src_len_ptr18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_src_load, i32 0, i32 0
  %src_data_ptr_field19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_src_load, i32 0, i32 2
  %length20 = load i64, ptr %src_len_ptr18, align 4
  %src_data_ptr21 = load ptr, ptr %src_data_ptr_field19, align 8
  %new_header22 = call ptr @malloc(i64 24)
  %byte_count23 = mul i64 %length20, 8
  %10 = icmp eq i64 %length20, 0
  %11 = select i1 %10, i64 32, i64 %byte_count23
  %new_data24 = call ptr @malloc(i64 %11)
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header22, i32 0, i32 0
  store i64 %length20, ptr %12, align 4
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header22, i32 0, i32 1
  store i64 %length20, ptr %13, align 4
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header22, i32 0, i32 2
  store ptr %new_data24, ptr %14, align 8
  %15 = icmp ugt i64 %length20, 0
  br i1 %15, label %copy.do25, label %copy.end26

array.copy.loop:                                  ; preds = %array.copy.loop, %copy.do
  %i = load i64, ptr %index_ptr, align 4
  %src_elem_p = getelementptr ptr, ptr %src_data_ptr, i64 %i
  %src_elem_v = load ptr, ptr %src_elem_p, align 8
  %record_copy_buffer = call ptr @malloc(i64 40)
  %src_slot = getelementptr ptr, ptr %src_elem_v, i64 0
  %stored_ptr = load ptr, ptr %src_slot, align 8
  %dst_slot = getelementptr ptr, ptr %record_copy_buffer, i64 0
  store ptr %stored_ptr, ptr %dst_slot, align 8
  %src_slot2 = getelementptr ptr, ptr %src_elem_v, i64 1
  %stored_ptr3 = load ptr, ptr %src_slot2, align 8
  %dst_slot4 = getelementptr ptr, ptr %record_copy_buffer, i64 1
  store ptr %stored_ptr3, ptr %dst_slot4, align 8
  %src_slot5 = getelementptr ptr, ptr %src_elem_v, i64 2
  %stored_ptr6 = load ptr, ptr %src_slot5, align 8
  %primitive_box_copy = call ptr @malloc(i64 8)
  %16 = bitcast ptr %primitive_box_copy to ptr
  %box_val = load i64, ptr %stored_ptr6, align 4
  store i64 %box_val, ptr %16, align 4
  %dst_slot7 = getelementptr ptr, ptr %record_copy_buffer, i64 2
  store ptr %primitive_box_copy, ptr %dst_slot7, align 8
  %src_slot8 = getelementptr ptr, ptr %src_elem_v, i64 3
  %stored_ptr9 = load ptr, ptr %src_slot8, align 8
  %primitive_box_copy10 = call ptr @malloc(i64 8)
  %17 = bitcast ptr %primitive_box_copy10 to ptr
  %box_val11 = load i1, ptr %stored_ptr9, align 1
  store i1 %box_val11, ptr %17, align 1
  %dst_slot12 = getelementptr ptr, ptr %record_copy_buffer, i64 3
  store ptr %primitive_box_copy10, ptr %dst_slot12, align 8
  %src_slot13 = getelementptr ptr, ptr %src_elem_v, i64 4
  %stored_ptr14 = load ptr, ptr %src_slot13, align 8
  %primitive_box_copy15 = call ptr @malloc(i64 8)
  %18 = bitcast ptr %primitive_box_copy15 to ptr
  %box_val16 = load double, ptr %stored_ptr14, align 8
  store double %box_val16, ptr %18, align 8
  %dst_slot17 = getelementptr ptr, ptr %record_copy_buffer, i64 4
  store ptr %primitive_box_copy15, ptr %dst_slot17, align 8
  %dst_elem_p = getelementptr ptr, ptr %9, i64 %i
  store ptr %record_copy_buffer, ptr %dst_elem_p, align 8
  %next_i = add i64 %i, 1
  store i64 %next_i, ptr %index_ptr, align 4
  %loop_cond = icmp ult i64 %next_i, %length
  br i1 %loop_cond, label %array.copy.loop, label %array.copy.exit

array.copy.exit:                                  ; preds = %array.copy.loop
  br label %copy.end

copy.do25:                                        ; preds = %copy.end
  %19 = bitcast ptr %new_data24 to ptr
  %index_ptr29 = alloca i64, align 8
  store i64 0, ptr %index_ptr29, align 4
  br label %array.copy.loop27

copy.end26:                                       ; preds = %array.copy.exit28, %copy.end
  store ptr %new_header22, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond

array.copy.loop27:                                ; preds = %array.copy.loop27, %copy.do25
  %i30 = load i64, ptr %index_ptr29, align 4
  %src_elem_p31 = getelementptr ptr, ptr %src_data_ptr21, i64 %i30
  %src_elem_v32 = load ptr, ptr %src_elem_p31, align 8
  %record_copy_buffer33 = call ptr @malloc(i64 40)
  %src_slot34 = getelementptr ptr, ptr %src_elem_v32, i64 0
  %stored_ptr35 = load ptr, ptr %src_slot34, align 8
  %dst_slot36 = getelementptr ptr, ptr %record_copy_buffer33, i64 0
  store ptr %stored_ptr35, ptr %dst_slot36, align 8
  %src_slot37 = getelementptr ptr, ptr %src_elem_v32, i64 1
  %stored_ptr38 = load ptr, ptr %src_slot37, align 8
  %dst_slot39 = getelementptr ptr, ptr %record_copy_buffer33, i64 1
  store ptr %stored_ptr38, ptr %dst_slot39, align 8
  %src_slot40 = getelementptr ptr, ptr %src_elem_v32, i64 2
  %stored_ptr41 = load ptr, ptr %src_slot40, align 8
  %primitive_box_copy42 = call ptr @malloc(i64 8)
  %20 = bitcast ptr %primitive_box_copy42 to ptr
  %box_val43 = load i64, ptr %stored_ptr41, align 4
  store i64 %box_val43, ptr %20, align 4
  %dst_slot44 = getelementptr ptr, ptr %record_copy_buffer33, i64 2
  store ptr %primitive_box_copy42, ptr %dst_slot44, align 8
  %src_slot45 = getelementptr ptr, ptr %src_elem_v32, i64 3
  %stored_ptr46 = load ptr, ptr %src_slot45, align 8
  %primitive_box_copy47 = call ptr @malloc(i64 8)
  %21 = bitcast ptr %primitive_box_copy47 to ptr
  %box_val48 = load i1, ptr %stored_ptr46, align 1
  store i1 %box_val48, ptr %21, align 1
  %dst_slot49 = getelementptr ptr, ptr %record_copy_buffer33, i64 3
  store ptr %primitive_box_copy47, ptr %dst_slot49, align 8
  %src_slot50 = getelementptr ptr, ptr %src_elem_v32, i64 4
  %stored_ptr51 = load ptr, ptr %src_slot50, align 8
  %primitive_box_copy52 = call ptr @malloc(i64 8)
  %22 = bitcast ptr %primitive_box_copy52 to ptr
  %box_val53 = load double, ptr %stored_ptr51, align 8
  store double %box_val53, ptr %22, align 8
  %dst_slot54 = getelementptr ptr, ptr %record_copy_buffer33, i64 4
  store ptr %primitive_box_copy52, ptr %dst_slot54, align 8
  %dst_elem_p55 = getelementptr ptr, ptr %19, i64 %i30
  store ptr %record_copy_buffer33, ptr %dst_elem_p55, align 8
  %next_i56 = add i64 %i30, 1
  store i64 %next_i56, ptr %index_ptr29, align 4
  %loop_cond57 = icmp ult i64 %next_i56, %length20
  br i1 %loop_cond57, label %array.copy.loop27, label %array.copy.exit28

array.copy.exit28:                                ; preds = %array.copy.loop27
  br label %copy.end26

for.cond:                                         ; preds = %for.step, %copy.end26
  %__map_i_load = load i64, ptr @__map_i, align 8
  %__map_src_load58 = load ptr, ptr @__map_src, align 8
  %length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_src_load58, i32 0, i32 0
  %length59 = load i64, ptr %length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__map_i_load, %length59
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %__map_i_load60 = load i64, ptr @__map_i, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 0
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 2
  %array_len = load i64, ptr %len_ptr, align 4
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %23 = icmp slt i64 %__map_i_load60, 0
  %24 = icmp sge i64 %__map_i_load60, %array_len
  %is_invalid = or i1 %23, %24
  br i1 %is_invalid, label %assign_bounds.fail, label %assign_bounds.ok

for.step:                                         ; preds = %bounds.ok76
  %inc_load = load i64, ptr @__map_i, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__map_result_load82 = load ptr, ptr @__map_result, align 8
  %"$src_rows_load83" = load ptr, ptr @"$src_rows", align 8
  %src_cols_gep = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %"$src_rows_load83", i32 0, i32 0
  %src_types_gep = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %"$src_rows_load83", i32 0, i32 2
  %col_names = load ptr, ptr %src_cols_gep, align 8
  %col_types = load ptr, ptr %src_types_gep, align 8
  %df_header_raw = call ptr @malloc(i64 24)
  %df_ptr = bitcast ptr %df_header_raw to ptr
  %dest_cols_gep = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr, i32 0, i32 0
  %dest_rows_gep = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr, i32 0, i32 1
  %dest_types_gep = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr, i32 0, i32 2
  store ptr %col_names, ptr %dest_cols_gep, align 8
  store ptr %__map_result_load82, ptr %dest_rows_gep, align 8
  store ptr %col_types, ptr %dest_types_gep, align 8
  %tags_data = call ptr @malloc(i64 16)
  %tag_slot_0 = getelementptr i8, ptr %tags_data, i64 0
  %tag_cast_0 = bitcast ptr %tag_slot_0 to ptr
  store i64 4, ptr %tag_cast_0, align 4
  %tag_slot_1 = getelementptr i8, ptr %tags_data, i64 8
  %tag_cast_1 = bitcast ptr %tag_slot_1 to ptr
  store i64 1, ptr %tag_cast_1, align 4
  %tags_header = call ptr @malloc(i64 24)
  %25 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %tags_header, i32 0, i32 0
  store i64 2, ptr %25, align 4
  %26 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %tags_header, i32 0, i32 1
  store i64 2, ptr %26, align 4
  %27 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %tags_header, i32 0, i32 2
  store ptr %tags_data, ptr %27, align 8
  %df_ptr84 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %28 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr84, i32 0, i32 0
  store ptr %arr_header, ptr %28, align 8
  %29 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr84, i32 0, i32 1
  store ptr %df_ptr, ptr %29, align 8
  %30 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_ptr84, i32 0, i32 2
  store ptr %tags_header, ptr %30, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 8
  %data_ptr85 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %df_ptr84, ptr %data_ptr85, align 8
  ret ptr %runtime_obj

assign_bounds.fail:                               ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

assign_bounds.ok:                                 ; preds = %for.body
  %record_buffer = call ptr @malloc(i64 16)
  %__map_src_load61 = load ptr, ptr @__map_src, align 8
  %__map_i_load62 = load i64, ptr @__map_i, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_src_load61, i32 0, i32 0
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_src_load61, i32 0, i32 2
  %array_len63 = load i64, ptr %len_field_ptr, align 4
  %data_ptr64 = load ptr, ptr %data_field_ptr, align 8
  %31 = icmp slt i64 %__map_i_load62, 0
  %32 = icmp sge i64 %__map_i_load62, %array_len63
  %is_invalid65 = or i1 %31, %32
  br i1 %is_invalid65, label %bounds.fail, label %bounds.ok

bounds.fail:                                      ; preds = %assign_bounds.ok
  %print_err66 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok:                                        ; preds = %assign_bounds.ok
  %elem_ptr67 = getelementptr ptr, ptr %data_ptr64, i64 %__map_i_load62
  %raw_val = load ptr, ptr %elem_ptr67, align 8
  %gep_name = getelementptr ptr, ptr %raw_val, i64 1
  %ptr_to_name = load ptr, ptr %gep_name, align 8
  %field_ptr = getelementptr ptr, ptr %record_buffer, i64 0
  store ptr %ptr_to_name, ptr %field_ptr, align 8
  %__map_src_load68 = load ptr, ptr @__map_src, align 8
  %__map_i_load69 = load i64, ptr @__map_i, align 8
  %len_field_ptr70 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_src_load68, i32 0, i32 0
  %data_field_ptr71 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_src_load68, i32 0, i32 2
  %array_len72 = load i64, ptr %len_field_ptr70, align 4
  %data_ptr73 = load ptr, ptr %data_field_ptr71, align 8
  %33 = icmp slt i64 %__map_i_load69, 0
  %34 = icmp sge i64 %__map_i_load69, %array_len72
  %is_invalid74 = or i1 %33, %34
  br i1 %is_invalid74, label %bounds.fail75, label %bounds.ok76

bounds.fail75:                                    ; preds = %bounds.ok
  %print_err77 = call i32 (ptr, ...) @printf(ptr @err_msg.3)
  ret ptr null

bounds.ok76:                                      ; preds = %bounds.ok
  %elem_ptr78 = getelementptr ptr, ptr %data_ptr73, i64 %__map_i_load69
  %raw_val79 = load ptr, ptr %elem_ptr78, align 8
  %gep_age = getelementptr ptr, ptr %raw_val79, i64 2
  %ptr_to_age = load ptr, ptr %gep_age, align 8
  %val_age = load i64, ptr %ptr_to_age, align 4
  %field_mem = call ptr @malloc(i64 8)
  %cast = bitcast ptr %field_mem to ptr
  store i64 %val_age, ptr %cast, align 4
  %field_ptr80 = getelementptr ptr, ptr %record_buffer, i64 1
  store ptr %field_mem, ptr %field_ptr80, align 8
  %val_to_ptr = bitcast ptr %record_buffer to ptr
  %elem_ptr81 = getelementptr ptr, ptr %data_ptr, i64 %__map_i_load60
  store ptr %val_to_ptr, ptr %elem_ptr81, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
