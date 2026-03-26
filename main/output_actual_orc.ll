; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr
@__where_src = global ptr null
@__where_result = global ptr null
@__where_i = global i64 0
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_2() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__where_src, align 8
  %arr_header_raw = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 32)
  %len_field = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header_raw, i32 0, i32 0
  store i64 0, ptr %len_field, align 4
  %cap_field = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header_raw, i32 0, i32 1
  store i64 4, ptr %cap_field, align 4
  %data_ptr_field = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header_raw, i32 0, i32 2
  store ptr %arr_data, ptr %data_ptr_field, align 8
  store ptr %arr_header_raw, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__where_i_load = load i64, ptr @__where_i, align 4
  %__where_src_load = load ptr, ptr @__where_src, align 8
  %len_ptr = getelementptr i64, ptr %__where_src_load, i32 0
  %length = load i64, ptr %len_ptr, align 8
  %icmp_tmp = icmp slt i64 %__where_i_load, %length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__where_src_load1 = load ptr, ptr @__where_src, align 8
  %__where_i_load2 = load i64, ptr @__where_i, align 4
  %len_ptr3 = getelementptr i64, ptr %__where_src_load1, i32 0
  %array_len = load i64, ptr %len_ptr3, align 4
  %is_neg = icmp slt i64 %__where_i_load2, 0
  %is_too_big = icmp sge i64 %__where_i_load2, %array_len
  %is_invalid = or i1 %is_neg, %is_too_big
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %ifcont
  %inc_load = load i64, ptr @__where_i, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__where_result_load19 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 5, ptr %tag_ptr, align 2
  %data_ptr20 = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__where_result_load19, ptr %data_ptr20, align 8
  ret ptr %runtime_obj

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %offset_idx = add i64 %__where_i_load2, 2
  %elem_ptr = getelementptr i64, ptr %__where_src_load1, i64 %offset_idx
  %raw_val = load i64, ptr %elem_ptr, align 8
  %icmp_tmp4 = icmp sgt i64 %raw_val, 7
  br i1 %icmp_tmp4, label %then, label %else

then:                                             ; preds = %bounds.ok
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load5 = load ptr, ptr @__where_src, align 8
  %__where_i_load6 = load i64, ptr @__where_i, align 4
  %len_ptr7 = getelementptr i64, ptr %__where_src_load5, i32 0
  %array_len8 = load i64, ptr %len_ptr7, align 4
  %is_neg9 = icmp slt i64 %__where_i_load6, 0
  %is_too_big10 = icmp sge i64 %__where_i_load6, %array_len8
  %is_invalid11 = or i1 %is_neg9, %is_too_big10
  br i1 %is_invalid11, label %bounds.fail12, label %bounds.ok13

else:                                             ; preds = %bounds.ok
  br label %ifcont

ifcont:                                           ; preds = %else, %add_cont
  %iftmp = phi ptr [ %__where_result_load, %add_cont ], [ 0.000000e+00, %else ]
  br label %for.step

bounds.fail12:                                    ; preds = %then
  %print_err14 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok13:                                      ; preds = %then
  %offset_idx15 = add i64 %__where_i_load6, 2
  %elem_ptr16 = getelementptr i64, ptr %__where_src_load5, i64 %offset_idx15
  %raw_val17 = load i64, ptr %elem_ptr16, align 8
  %val_to_add = bitcast i64 %raw_val17 to ptr
  %len_ptr18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load, i32 0, i32 2
  %len = load i64, ptr %len_ptr18, align 4
  %cap = load i64, ptr %cap_ptr, align 4
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %is_full = icmp eq i64 %len, %cap
  br i1 %is_full, label %grow, label %add_cont

grow:                                             ; preds = %bounds.ok13
  %new_cap = mul i64 %cap, 2
  %0 = icmp eq i64 %cap, 0
  %final_new_cap = select i1 %0, i64 4, i64 %new_cap
  %1 = mul i64 %final_new_cap, 8
  %realloc_ptr = call ptr @realloc(ptr %data_ptr, i64 %1)
  store i64 %final_new_cap, ptr %cap_ptr, align 4
  store ptr %realloc_ptr, ptr %data_ptr_ptr, align 8
  br label %add_cont

add_cont:                                         ; preds = %grow, %bounds.ok13
  %final_data_ptr = phi ptr [ %data_ptr, %bounds.ok13 ], [ %realloc_ptr, %grow ]
  %target_elem_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %val_to_add, ptr %target_elem_ptr, align 8
  %2 = add i64 %len, 1
  store i64 %2, ptr %len_ptr18, align 4
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
