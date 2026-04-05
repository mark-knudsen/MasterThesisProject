; ModuleID = 'repl_module'
source_filename = "repl_module"

@df2 = external global ptr
@__map_src = global ptr null, align 8
@__map_result = global ptr null, align 8
@__map_i = global i64 0, align 8
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@x = external global ptr
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.2 = private unnamed_addr constant [7 x i8] c"hasJob\00", align 1
@str.3 = private unnamed_addr constant [8 x i8] c"savings\00", align 1

define ptr @main_2() {
entry:
  %df2_load = load ptr, ptr @df2, align 8
  %df_field_extract = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df2_load, i32 0, i32 1
  %arr_header_ptr = load ptr, ptr %df_field_extract, align 8
  %src_len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header_ptr, i32 0, i32 0
  %src_data_ptr_field = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header_ptr, i32 0, i32 2
  %length = load i64, ptr %src_len_ptr, align 4
  %src_data_ptr = load ptr, ptr %src_data_ptr_field, align 8
  %new_header = call ptr @malloc(i64 24)
  %byte_count = mul i64 %length, 8
  %0 = icmp eq i64 %length, 0
  %1 = select i1 %0, i64 32, i64 %byte_count
  %new_data = call ptr @malloc(i64 %1)
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header, i32 0, i32 0
  store i64 %length, ptr %2, align 4
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header, i32 0, i32 1
  store i64 %length, ptr %3, align 4
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header, i32 0, i32 2
  store ptr %new_data, ptr %4, align 8
  %5 = icmp ugt i64 %length, 0
  br i1 %5, label %copy.do, label %copy.end

copy.do:                                          ; preds = %entry
  %6 = bitcast ptr %new_data to ptr
  %index_ptr = alloca i64, align 8
  store i64 0, ptr %index_ptr, align 4
  br label %array.copy.loop

copy.end:                                         ; preds = %array.copy.exit, %entry
  store ptr %new_header, ptr @__map_src, align 8
  %__map_src_load = load ptr, ptr @__map_src, align 8
  %src_len_ptr17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_src_load, i32 0, i32 0
  %src_data_ptr_field18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_src_load, i32 0, i32 2
  %length19 = load i64, ptr %src_len_ptr17, align 4
  %src_data_ptr20 = load ptr, ptr %src_data_ptr_field18, align 8
  %new_header21 = call ptr @malloc(i64 24)
  %byte_count22 = mul i64 %length19, 8
  %7 = icmp eq i64 %length19, 0
  %8 = select i1 %7, i64 32, i64 %byte_count22
  %new_data23 = call ptr @malloc(i64 %8)
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header21, i32 0, i32 0
  store i64 %length19, ptr %9, align 4
  %10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header21, i32 0, i32 1
  store i64 %length19, ptr %10, align 4
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header21, i32 0, i32 2
  store ptr %new_data23, ptr %11, align 8
  %12 = icmp ugt i64 %length19, 0
  br i1 %12, label %copy.do24, label %copy.end25

array.copy.loop:                                  ; preds = %array.copy.loop, %copy.do
  %i = load i64, ptr %index_ptr, align 4
  %src_elem_p = getelementptr ptr, ptr %src_data_ptr, i64 %i
  %src_elem_v = load ptr, ptr %src_elem_p, align 8
  %record_copy_buffer = call ptr @malloc(i64 40)
  %src_slot = getelementptr ptr, ptr %src_elem_v, i64 0
  %stored_ptr = load ptr, ptr %src_slot, align 8
  %dst_slot = getelementptr ptr, ptr %record_copy_buffer, i64 0
  store ptr %stored_ptr, ptr %dst_slot, align 8
  %src_slot1 = getelementptr ptr, ptr %src_elem_v, i64 1
  %stored_ptr2 = load ptr, ptr %src_slot1, align 8
  %dst_slot3 = getelementptr ptr, ptr %record_copy_buffer, i64 1
  store ptr %stored_ptr2, ptr %dst_slot3, align 8
  %src_slot4 = getelementptr ptr, ptr %src_elem_v, i64 2
  %stored_ptr5 = load ptr, ptr %src_slot4, align 8
  %primitive_box_copy = call ptr @malloc(i64 8)
  %13 = bitcast ptr %primitive_box_copy to ptr
  %box_val = load i64, ptr %stored_ptr5, align 4
  store i64 %box_val, ptr %13, align 4
  %dst_slot6 = getelementptr ptr, ptr %record_copy_buffer, i64 2
  store ptr %primitive_box_copy, ptr %dst_slot6, align 8
  %src_slot7 = getelementptr ptr, ptr %src_elem_v, i64 3
  %stored_ptr8 = load ptr, ptr %src_slot7, align 8
  %primitive_box_copy9 = call ptr @malloc(i64 8)
  %14 = bitcast ptr %primitive_box_copy9 to ptr
  %box_val10 = load i1, ptr %stored_ptr8, align 1
  store i1 %box_val10, ptr %14, align 1
  %dst_slot11 = getelementptr ptr, ptr %record_copy_buffer, i64 3
  store ptr %primitive_box_copy9, ptr %dst_slot11, align 8
  %src_slot12 = getelementptr ptr, ptr %src_elem_v, i64 4
  %stored_ptr13 = load ptr, ptr %src_slot12, align 8
  %primitive_box_copy14 = call ptr @malloc(i64 8)
  %15 = bitcast ptr %primitive_box_copy14 to ptr
  %box_val15 = load double, ptr %stored_ptr13, align 8
  store double %box_val15, ptr %15, align 8
  %dst_slot16 = getelementptr ptr, ptr %record_copy_buffer, i64 4
  store ptr %primitive_box_copy14, ptr %dst_slot16, align 8
  %dst_elem_p = getelementptr ptr, ptr %6, i64 %i
  store ptr %record_copy_buffer, ptr %dst_elem_p, align 8
  %next_i = add i64 %i, 1
  store i64 %next_i, ptr %index_ptr, align 4
  %loop_cond = icmp ult i64 %next_i, %length
  br i1 %loop_cond, label %array.copy.loop, label %array.copy.exit

array.copy.exit:                                  ; preds = %array.copy.loop
  br label %copy.end

copy.do24:                                        ; preds = %copy.end
  %16 = bitcast ptr %new_data23 to ptr
  %index_ptr28 = alloca i64, align 8
  store i64 0, ptr %index_ptr28, align 4
  br label %array.copy.loop26

copy.end25:                                       ; preds = %array.copy.exit27, %copy.end
  store ptr %new_header21, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond

array.copy.loop26:                                ; preds = %array.copy.loop26, %copy.do24
  %i29 = load i64, ptr %index_ptr28, align 4
  %src_elem_p30 = getelementptr ptr, ptr %src_data_ptr20, i64 %i29
  %src_elem_v31 = load ptr, ptr %src_elem_p30, align 8
  %record_copy_buffer32 = call ptr @malloc(i64 40)
  %src_slot33 = getelementptr ptr, ptr %src_elem_v31, i64 0
  %stored_ptr34 = load ptr, ptr %src_slot33, align 8
  %dst_slot35 = getelementptr ptr, ptr %record_copy_buffer32, i64 0
  store ptr %stored_ptr34, ptr %dst_slot35, align 8
  %src_slot36 = getelementptr ptr, ptr %src_elem_v31, i64 1
  %stored_ptr37 = load ptr, ptr %src_slot36, align 8
  %dst_slot38 = getelementptr ptr, ptr %record_copy_buffer32, i64 1
  store ptr %stored_ptr37, ptr %dst_slot38, align 8
  %src_slot39 = getelementptr ptr, ptr %src_elem_v31, i64 2
  %stored_ptr40 = load ptr, ptr %src_slot39, align 8
  %primitive_box_copy41 = call ptr @malloc(i64 8)
  %17 = bitcast ptr %primitive_box_copy41 to ptr
  %box_val42 = load i64, ptr %stored_ptr40, align 4
  store i64 %box_val42, ptr %17, align 4
  %dst_slot43 = getelementptr ptr, ptr %record_copy_buffer32, i64 2
  store ptr %primitive_box_copy41, ptr %dst_slot43, align 8
  %src_slot44 = getelementptr ptr, ptr %src_elem_v31, i64 3
  %stored_ptr45 = load ptr, ptr %src_slot44, align 8
  %primitive_box_copy46 = call ptr @malloc(i64 8)
  %18 = bitcast ptr %primitive_box_copy46 to ptr
  %box_val47 = load i1, ptr %stored_ptr45, align 1
  store i1 %box_val47, ptr %18, align 1
  %dst_slot48 = getelementptr ptr, ptr %record_copy_buffer32, i64 3
  store ptr %primitive_box_copy46, ptr %dst_slot48, align 8
  %src_slot49 = getelementptr ptr, ptr %src_elem_v31, i64 4
  %stored_ptr50 = load ptr, ptr %src_slot49, align 8
  %primitive_box_copy51 = call ptr @malloc(i64 8)
  %19 = bitcast ptr %primitive_box_copy51 to ptr
  %box_val52 = load double, ptr %stored_ptr50, align 8
  store double %box_val52, ptr %19, align 8
  %dst_slot53 = getelementptr ptr, ptr %record_copy_buffer32, i64 4
  store ptr %primitive_box_copy51, ptr %dst_slot53, align 8
  %dst_elem_p54 = getelementptr ptr, ptr %16, i64 %i29
  store ptr %record_copy_buffer32, ptr %dst_elem_p54, align 8
  %next_i55 = add i64 %i29, 1
  store i64 %next_i55, ptr %index_ptr28, align 4
  %loop_cond56 = icmp ult i64 %next_i55, %length19
  br i1 %loop_cond56, label %array.copy.loop26, label %array.copy.exit27

array.copy.exit27:                                ; preds = %array.copy.loop26
  br label %copy.end25

for.cond:                                         ; preds = %for.step, %copy.end25
  %__map_i_load = load i64, ptr @__map_i, align 8
  %__map_src_load57 = load ptr, ptr @__map_src, align 8
  %length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_src_load57, i32 0, i32 0
  %length58 = load i64, ptr %length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__map_i_load, %length58
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %__map_i_load59 = load i64, ptr @__map_i, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 0
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__map_result_load, i32 0, i32 2
  %array_len = load i64, ptr %len_ptr, align 4
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %20 = icmp slt i64 %__map_i_load59, 0
  %21 = icmp sge i64 %__map_i_load59, %array_len
  %is_invalid = or i1 %20, %21
  br i1 %is_invalid, label %assign_bounds.fail, label %assign_bounds.ok

for.step:                                         ; preds = %assign_bounds.ok
  %inc_load = load i64, ptr @__map_i, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__map_result_load69 = load ptr, ptr @__map_result, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 64)
  %22 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 4, ptr %22, align 4
  %23 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 8, ptr %23, align 4
  %24 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %24, align 8
  %elem_ptr70 = getelementptr ptr, ptr %arr_data, i64 0
  store ptr @str, ptr %elem_ptr70, align 8
  %elem_ptr71 = getelementptr ptr, ptr %arr_data, i64 1
  store ptr @str.1, ptr %elem_ptr71, align 8
  %elem_ptr72 = getelementptr ptr, ptr %arr_data, i64 2
  store ptr @str.2, ptr %elem_ptr72, align 8
  %elem_ptr73 = getelementptr ptr, ptr %arr_data, i64 3
  store ptr @str.3, ptr %elem_ptr73, align 8
  %arr_header74 = call ptr @malloc(i64 24)
  %arr_data75 = call ptr @malloc(i64 64)
  %25 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header74, i32 0, i32 0
  store i64 4, ptr %25, align 4
  %26 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header74, i32 0, i32 1
  store i64 8, ptr %26, align 4
  %27 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header74, i32 0, i32 2
  store ptr %arr_data75, ptr %27, align 8
  %elem_ptr76 = getelementptr ptr, ptr %arr_data75, i64 0
  store ptr bitcast (i64 4 to ptr), ptr %elem_ptr76, align 8
  %elem_ptr77 = getelementptr ptr, ptr %arr_data75, i64 1
  store ptr bitcast (i64 1 to ptr), ptr %elem_ptr77, align 8
  %elem_ptr78 = getelementptr ptr, ptr %arr_data75, i64 2
  store ptr null, ptr %elem_ptr78, align 8
  %elem_ptr79 = getelementptr ptr, ptr %arr_data75, i64 3
  store ptr bitcast (i64 2 to ptr), ptr %elem_ptr79, align 8
  %df_header = call ptr @malloc(i64 24)
  %28 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_header, i32 0, i32 0
  store ptr %arr_header, ptr %28, align 8
  %29 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_header, i32 0, i32 1
  store ptr %__map_result_load69, ptr %29, align 8
  %30 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_header, i32 0, i32 2
  store ptr %arr_header74, ptr %30, align 8
  %boxed_ptr_cast = bitcast ptr %df_header to ptr
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 8
  %data_ptr80 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %boxed_ptr_cast, ptr %data_ptr80, align 8
  %runtime_obj81 = call ptr @malloc(i64 16)
  %runtime_cast82 = bitcast ptr %runtime_obj81 to ptr
  %tag_ptr83 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast82, i32 0, i32 0
  store i16 5, ptr %tag_ptr83, align 8
  %data_ptr84 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast82, i32 0, i32 1
  store ptr %runtime_obj, ptr %data_ptr84, align 8
  ret ptr %runtime_obj81

assign_bounds.fail:                               ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

assign_bounds.ok:                                 ; preds = %for.body
  %record_buffer = call ptr @malloc(i64 32)
  %x_load = load ptr, ptr @x, align 8
  %ptr_name = getelementptr ptr, ptr %x_load, i64 1
  %load_ptr_name = load ptr, ptr %ptr_name, align 8
  %field_ptr = getelementptr ptr, ptr %record_buffer, i64 0
  store ptr %load_ptr_name, ptr %field_ptr, align 8
  %field_mem = call ptr @malloc(i64 8)
  %cast = bitcast ptr %field_mem to ptr
  store i64 10, ptr %cast, align 4
  %field_ptr60 = getelementptr ptr, ptr %record_buffer, i64 1
  store ptr %field_mem, ptr %field_ptr60, align 8
  %x_load61 = load ptr, ptr @x, align 8
  %ptr_hasJob = getelementptr ptr, ptr %x_load61, i64 3
  %load_ptr_hasJob = load ptr, ptr %ptr_hasJob, align 8
  %val_hasJob = load i1, ptr %load_ptr_hasJob, align 1
  %field_mem62 = call ptr @malloc(i64 8)
  %cast63 = bitcast ptr %field_mem62 to ptr
  store i1 %val_hasJob, ptr %cast63, align 1
  %field_ptr64 = getelementptr ptr, ptr %record_buffer, i64 2
  store ptr %field_mem62, ptr %field_ptr64, align 8
  %x_load65 = load ptr, ptr @x, align 8
  %ptr_savings = getelementptr ptr, ptr %x_load65, i64 4
  %load_ptr_savings = load ptr, ptr %ptr_savings, align 8
  %val_savings = load double, ptr %load_ptr_savings, align 8
  %field_mem66 = call ptr @malloc(i64 8)
  %cast67 = bitcast ptr %field_mem66 to ptr
  store double %val_savings, ptr %cast67, align 8
  %field_ptr68 = getelementptr ptr, ptr %record_buffer, i64 3
  store ptr %field_mem66, ptr %field_ptr68, align 8
  %val_to_ptr = bitcast ptr %record_buffer to ptr
  %elem_ptr = getelementptr ptr, ptr %data_ptr, i64 %__map_i_load59
  store ptr %val_to_ptr, ptr %elem_ptr, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
