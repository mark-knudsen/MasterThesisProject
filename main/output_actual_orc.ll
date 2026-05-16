; ModuleID = 'repl_module'
source_filename = "repl_module"

@df2 = external global ptr
@df3 = global ptr null, align 8

define ptr @main_2() {
entry:
  %df2_load = load ptr, ptr @df2, align 8
  %df_copy_header = call ptr @malloc(i64 24)
  %0 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df2_load, i32 0, i32 0
  %ld_cols = load ptr, ptr %0, align 8
  %src_len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %ld_cols, i32 0, i32 0
  %src_data_ptr_field = getelementptr inbounds nuw { i64, i64, ptr }, ptr %ld_cols, i32 0, i32 2
  %length = load i64, ptr %src_len_ptr, align 4
  %src_data_ptr = load ptr, ptr %src_data_ptr_field, align 8
  %new_header = call ptr @malloc(i64 24)
  %byte_count = mul i64 %length, 8
  %1 = icmp eq i64 %length, 0
  %2 = select i1 %1, i64 32, i64 %byte_count
  %new_data = call ptr @malloc(i64 %2)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header, i32 0, i32 0
  store i64 %length, ptr %3, align 4
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header, i32 0, i32 1
  store i64 %length, ptr %4, align 4
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header, i32 0, i32 2
  store ptr %new_data, ptr %5, align 8
  %6 = icmp ugt i64 %length, 0
  br i1 %6, label %copy.do, label %copy.end

copy.do:                                          ; preds = %entry
  call void @memmove(ptr %new_data, ptr %src_data_ptr, i64 %byte_count)
  br label %copy.end

copy.end:                                         ; preds = %copy.do, %entry
  %7 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_copy_header, i32 0, i32 0
  store ptr %new_header, ptr %7, align 8
  %8 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df2_load, i32 0, i32 1
  %ld_rows = load ptr, ptr %8, align 8
  %src_len_ptr1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %ld_rows, i32 0, i32 0
  %src_data_ptr_field2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %ld_rows, i32 0, i32 2
  %length3 = load i64, ptr %src_len_ptr1, align 4
  %src_data_ptr4 = load ptr, ptr %src_data_ptr_field2, align 8
  %new_header5 = call ptr @malloc(i64 24)
  %byte_count6 = mul i64 %length3, 8
  %9 = icmp eq i64 %length3, 0
  %10 = select i1 %9, i64 32, i64 %byte_count6
  %new_data7 = call ptr @malloc(i64 %10)
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header5, i32 0, i32 0
  store i64 %length3, ptr %11, align 4
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header5, i32 0, i32 1
  store i64 %length3, ptr %12, align 4
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header5, i32 0, i32 2
  store ptr %new_data7, ptr %13, align 8
  %14 = icmp ugt i64 %length3, 0
  br i1 %14, label %copy.do8, label %copy.end9

copy.do8:                                         ; preds = %copy.end
  %index_ptr = alloca i64, align 8
  store i64 0, ptr %index_ptr, align 4
  br label %array.copy.loop

copy.end9:                                        ; preds = %array.copy.exit, %copy.end
  %15 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_copy_header, i32 0, i32 1
  store ptr %new_header5, ptr %15, align 8
  %16 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df2_load, i32 0, i32 2
  %ld_types = load ptr, ptr %16, align 8
  %src_len_ptr10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %ld_types, i32 0, i32 0
  %src_data_ptr_field11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %ld_types, i32 0, i32 2
  %length12 = load i64, ptr %src_len_ptr10, align 4
  %src_data_ptr13 = load ptr, ptr %src_data_ptr_field11, align 8
  %new_header14 = call ptr @malloc(i64 24)
  %byte_count15 = mul i64 %length12, 8
  %17 = icmp eq i64 %length12, 0
  %18 = select i1 %17, i64 32, i64 %byte_count15
  %new_data16 = call ptr @malloc(i64 %18)
  %19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header14, i32 0, i32 0
  store i64 %length12, ptr %19, align 4
  %20 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header14, i32 0, i32 1
  store i64 %length12, ptr %20, align 4
  %21 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %new_header14, i32 0, i32 2
  store ptr %new_data16, ptr %21, align 8
  %22 = icmp ugt i64 %length12, 0
  br i1 %22, label %copy.do17, label %copy.end18

array.copy.loop:                                  ; preds = %array.copy.loop, %copy.do8
  %i = load i64, ptr %index_ptr, align 4
  %src_elem_p = getelementptr ptr, ptr %src_data_ptr4, i64 %i
  %src_elem_v = load ptr, ptr %src_elem_p, align 8
  %record_copy_mem = call ptr @malloc(i64 24)
  %src_f0_ptr = getelementptr inbounds nuw ptr, ptr %src_elem_v, i32 0, i32 0
  %dst_f0_ptr = getelementptr inbounds nuw ptr, ptr %record_copy_mem, i32 0, i32 0
  %f0_val = load ptr, ptr %src_f0_ptr, align 8
  store ptr %f0_val, ptr %dst_f0_ptr, align 8
  %src_f1_ptr = getelementptr inbounds nuw ptr, ptr %src_elem_v, i32 0, i32 1
  %dst_f1_ptr = getelementptr inbounds nuw ptr, ptr %record_copy_mem, i32 0, i32 1
  %f1_val = load i64, ptr %src_f1_ptr, align 8
  store i64 %f1_val, ptr %dst_f1_ptr, align 8
  %src_f2_ptr = getelementptr inbounds nuw ptr, ptr %src_elem_v, i32 0, i32 2
  %dst_f2_ptr = getelementptr inbounds nuw ptr, ptr %record_copy_mem, i32 0, i32 2
  %f2_val = load i8, ptr %src_f2_ptr, align 8
  store i8 %f2_val, ptr %dst_f2_ptr, align 8
  %dst_elem_p = getelementptr ptr, ptr %new_data7, i64 %i
  store ptr %record_copy_mem, ptr %dst_elem_p, align 8
  %next_i = add i64 %i, 1
  store i64 %next_i, ptr %index_ptr, align 4
  %loop_cond = icmp ult i64 %next_i, %length3
  br i1 %loop_cond, label %array.copy.loop, label %array.copy.exit

array.copy.exit:                                  ; preds = %array.copy.loop
  br label %copy.end9

copy.do17:                                        ; preds = %copy.end9
  call void @memmove(ptr %new_data16, ptr %src_data_ptr13, i64 %byte_count15)
  br label %copy.end18

copy.end18:                                       ; preds = %copy.do17, %copy.end9
  %23 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_copy_header, i32 0, i32 2
  store ptr %new_header14, ptr %23, align 8
  store ptr %df_copy_header, ptr @df3, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare void @memmove(ptr, ptr, i64)
