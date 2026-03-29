; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr
@__where_src = global ptr null
@__where_result = global ptr null
@__where_i = global i64 0
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_1() {
entry:
  %arr_load = load ptr, ptr @arr, align 8
  store ptr %arr_load, ptr @__where_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 32)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  store i64 0, ptr %0, align 4
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  store i64 4, ptr %1, align 4
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store ptr %arr_data, ptr %2, align 8
  store ptr %arr_header, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__where_i_load = load i64, ptr @__where_i, align 4
  %__where_src_load = load ptr, ptr @__where_src, align 8
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load, i32 0, i32 0
  %length = load i64, ptr %3, align 8
  %icmp_tmp = icmp slt i64 %__where_i_load, %length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__where_src_load1 = load ptr, ptr @__where_src, align 8
  %__where_i_load2 = load i64, ptr @__where_i, align 4
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load1, i32 0, i32 0
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load1, i32 0, i32 2
  %array_len = load i64, ptr %len_field_ptr, align 4
  %data_ptr = load ptr, ptr %data_field_ptr, align 8
  %4 = icmp slt i64 %__where_i_load2, 0
  %5 = icmp sge i64 %__where_i_load2, %array_len
  %is_invalid = or i1 %4, %5
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %ifcont
  %inc_load = load i64, ptr @__where_i, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__where_result_load17 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 5, ptr %tag_ptr, align 2
  %data_ptr18 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__where_result_load17, ptr %data_ptr18, align 8
  ret ptr %runtime_obj

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %elem_ptr = getelementptr ptr, ptr %data_ptr, i64 %__where_i_load2
  %raw_val = load ptr, ptr %elem_ptr, align 8
  %6 = ptrtoint ptr %raw_val to i64
  %icmp_tmp3 = icmp sgt i64 %6, 10
  br i1 %icmp_tmp3, label %then, label %else

then:                                             ; preds = %bounds.ok
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load4 = load ptr, ptr @__where_src, align 8
  %__where_i_load5 = load i64, ptr @__where_i, align 4
  %len_field_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load4, i32 0, i32 0
  %data_field_ptr7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load4, i32 0, i32 2
  %array_len8 = load i64, ptr %len_field_ptr6, align 4
  %data_ptr9 = load ptr, ptr %data_field_ptr7, align 8
  %7 = icmp slt i64 %__where_i_load5, 0
  %8 = icmp sge i64 %__where_i_load5, %array_len8
  %is_invalid10 = or i1 %7, %8
  br i1 %is_invalid10, label %bounds.fail11, label %bounds.ok12

else:                                             ; preds = %bounds.ok
  br label %ifcont

ifcont:                                           ; preds = %else, %add_cont
  %iftmp = phi ptr [ %__where_result_load, %add_cont ], [ 0.000000e+00, %else ]
  br label %for.step

bounds.fail11:                                    ; preds = %then
  %print_err13 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok12:                                      ; preds = %then
  %elem_ptr14 = getelementptr ptr, ptr %data_ptr9, i64 %__where_i_load5
  %raw_val15 = load ptr, ptr %elem_ptr14, align 8
  %9 = ptrtoint ptr %raw_val15 to i64
  %val_to_ptr = bitcast i64 %9 to ptr
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load, i32 0, i32 2
  %len = load i64, ptr %len_ptr, align 4
  %cap = load i64, ptr %cap_ptr, align 4
  %data_ptr16 = load ptr, ptr %data_ptr_ptr, align 8
  %is_full = icmp eq i64 %len, %cap
  br i1 %is_full, label %grow, label %add_cont

grow:                                             ; preds = %bounds.ok12
  %10 = icmp eq i64 %cap, 0
  %11 = mul i64 %cap, 2
  %new_cap = select i1 %10, i64 4, i64 %11
  %new_byte_size = mul i64 %new_cap, 8
  %realloc_ptr = call ptr @realloc(ptr %data_ptr16, i64 %new_byte_size)
  store i64 %new_cap, ptr %cap_ptr, align 4
  store ptr %realloc_ptr, ptr %data_ptr_ptr, align 8
  br label %add_cont

add_cont:                                         ; preds = %grow, %bounds.ok12
  %final_data_ptr = phi ptr [ %data_ptr16, %bounds.ok12 ], [ %realloc_ptr, %grow ]
  %target_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %val_to_ptr, ptr %target_ptr, align 8
  %12 = add i64 %len, 1
  store i64 %12, ptr %len_ptr, align 4
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
