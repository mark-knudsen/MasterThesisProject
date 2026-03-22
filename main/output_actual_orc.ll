; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr
@__where_src = external global ptr
@__where_result = external global ptr
@__where_i = external global i64
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.3 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_9() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__where_src, align 8
  %arr_ptr = call ptr @malloc(i64 16)
  %len_ptr = getelementptr i64, ptr %arr_ptr, i64 0
  store i64 0, ptr %len_ptr, align 8
  %cap_ptr = getelementptr i64, ptr %arr_ptr, i64 1
  store i64 0, ptr %cap_ptr, align 8
  store ptr %arr_ptr, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__where_i_load = load i64, ptr @__where_i, align 4
  %__where_src_load = load ptr, ptr @__where_src, align 8
  %len_ptr1 = getelementptr i64, ptr %__where_src_load, i32 0
  %length = load i64, ptr %len_ptr1, align 8
  %icmp_tmp = icmp slt i64 %__where_i_load, %length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__where_src_load2 = load ptr, ptr @__where_src, align 8
  %__where_i_load3 = load i64, ptr @__where_i, align 4
  %len_ptr4 = getelementptr i64, ptr %__where_src_load2, i32 0
  %array_len = load i64, ptr %len_ptr4, align 4
  %is_neg = icmp slt i64 %__where_i_load3, 0
  %is_too_big = icmp sge i64 %__where_i_load3, %array_len
  %is_invalid = or i1 %is_neg, %is_too_big
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %ifcont
  %inc_load = load i64, ptr @__where_i, align 4
  %inc_add = add i64 %inc_load, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__where_result_load24 = load ptr, ptr @__where_result, align 8
  store ptr %__where_result_load24, ptr @__where_src, align 8
  %arr_ptr25 = call ptr @malloc(i64 16)
  %len_ptr26 = getelementptr i64, ptr %arr_ptr25, i64 0
  store i64 0, ptr %len_ptr26, align 8
  %cap_ptr27 = getelementptr i64, ptr %arr_ptr25, i64 1
  store i64 0, ptr %cap_ptr27, align 8
  store ptr %arr_ptr25, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond28

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %offset_idx = add i64 %__where_i_load3, 2
  %elem_ptr = getelementptr i64, ptr %__where_src_load2, i64 %offset_idx
  %raw_val = load i64, ptr %elem_ptr, align 8
  %icmp_tmp5 = icmp sgt i64 %raw_val, 3
  br i1 %icmp_tmp5, label %then, label %else

then:                                             ; preds = %bounds.ok
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load6 = load ptr, ptr @__where_src, align 8
  %__where_i_load7 = load i64, ptr @__where_i, align 4
  %len_ptr8 = getelementptr i64, ptr %__where_src_load6, i32 0
  %array_len9 = load i64, ptr %len_ptr8, align 4
  %is_neg10 = icmp slt i64 %__where_i_load7, 0
  %is_too_big11 = icmp sge i64 %__where_i_load7, %array_len9
  %is_invalid12 = or i1 %is_neg10, %is_too_big11
  br i1 %is_invalid12, label %bounds.fail13, label %bounds.ok14

else:                                             ; preds = %bounds.ok
  br label %ifcont

ifcont:                                           ; preds = %else, %cont
  %iftmp = phi ptr [ %array_ptr_phi, %cont ], [ 0.000000e+00, %else ]
  br label %for.step

bounds.fail13:                                    ; preds = %then
  %print_err15 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok14:                                      ; preds = %then
  %offset_idx16 = add i64 %__where_i_load7, 2
  %elem_ptr17 = getelementptr i64, ptr %__where_src_load6, i64 %offset_idx16
  %raw_val18 = load i64, ptr %elem_ptr17, align 8
  %len_ptr19 = getelementptr i64, ptr %__where_result_load, i64 0
  %length20 = load i64, ptr %len_ptr19, align 8
  %cap_ptr21 = getelementptr i64, ptr %__where_result_load, i64 1
  %capacity = load i64, ptr %cap_ptr21, align 8
  %is_full = icmp eq i64 %length20, %capacity
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %bounds.ok14
  %new_capacity = mul i64 %capacity, 2
  %0 = icmp eq i64 %capacity, 0
  %new_capacity22 = select i1 %0, i64 4, i64 %new_capacity
  %slots = add i64 %new_capacity22, 2
  %total_bytes = mul i64 %slots, 8
  %realloc_array = call ptr @realloc(ptr %__where_result_load, i64 %total_bytes)
  %cap_ptr2 = getelementptr i64, ptr %realloc_array, i64 1
  store i64 %new_capacity22, ptr %cap_ptr2, align 8
  br label %cont

cont:                                             ; preds = %grow, %bounds.ok14
  %array_ptr_phi = phi ptr [ %__where_result_load, %bounds.ok14 ], [ %realloc_array, %grow ]
  %elem_index = add i64 %length20, 2
  %elem_ptr23 = getelementptr i64, ptr %array_ptr_phi, i64 %elem_index
  store i64 %raw_val18, ptr %elem_ptr23, align 8
  %new_length = add i64 %length20, 1
  %len_ptr2 = getelementptr i64, ptr %array_ptr_phi, i64 0
  store i64 %new_length, ptr %len_ptr2, align 8
  store ptr %array_ptr_phi, ptr @__where_result, align 8
  br label %ifcont

for.cond28:                                       ; preds = %for.step30, %for.end
  %__where_i_load32 = load i64, ptr @__where_i, align 4
  %__where_src_load33 = load ptr, ptr @__where_src, align 8
  %len_ptr34 = getelementptr i64, ptr %__where_src_load33, i32 0
  %length35 = load i64, ptr %len_ptr34, align 8
  %icmp_tmp36 = icmp slt i64 %__where_i_load32, %length35
  br i1 %icmp_tmp36, label %for.body29, label %for.end31

for.body29:                                       ; preds = %for.cond28
  %__where_src_load37 = load ptr, ptr @__where_src, align 8
  %__where_i_load38 = load i64, ptr @__where_i, align 4
  %len_ptr39 = getelementptr i64, ptr %__where_src_load37, i32 0
  %array_len40 = load i64, ptr %len_ptr39, align 4
  %is_neg41 = icmp slt i64 %__where_i_load38, 0
  %is_too_big42 = icmp sge i64 %__where_i_load38, %array_len40
  %is_invalid43 = or i1 %is_neg41, %is_too_big42
  br i1 %is_invalid43, label %bounds.fail44, label %bounds.ok45

for.step30:                                       ; preds = %ifcont53
  %inc_load87 = load i64, ptr @__where_i, align 4
  %inc_add88 = add i64 %inc_load87, 1
  store i64 %inc_add88, ptr @__where_i, align 8
  br label %for.cond28

for.end31:                                        ; preds = %for.cond28
  %__where_result_load89 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i16 5, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i16, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %__where_result_load89, ptr %data_ptr, align 8
  ret ptr %runtime_obj

bounds.fail44:                                    ; preds = %for.body29
  %print_err46 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok45:                                      ; preds = %for.body29
  %offset_idx47 = add i64 %__where_i_load38, 2
  %elem_ptr48 = getelementptr i64, ptr %__where_src_load37, i64 %offset_idx47
  %raw_val49 = load i64, ptr %elem_ptr48, align 8
  %icmp_tmp50 = icmp slt i64 %raw_val49, 7
  br i1 %icmp_tmp50, label %then51, label %else52

then51:                                           ; preds = %bounds.ok45
  %__where_result_load54 = load ptr, ptr @__where_result, align 8
  %__where_src_load55 = load ptr, ptr @__where_src, align 8
  %__where_i_load56 = load i64, ptr @__where_i, align 4
  %len_ptr57 = getelementptr i64, ptr %__where_src_load55, i32 0
  %array_len58 = load i64, ptr %len_ptr57, align 4
  %is_neg59 = icmp slt i64 %__where_i_load56, 0
  %is_too_big60 = icmp sge i64 %__where_i_load56, %array_len58
  %is_invalid61 = or i1 %is_neg59, %is_too_big60
  br i1 %is_invalid61, label %bounds.fail62, label %bounds.ok63

else52:                                           ; preds = %bounds.ok45
  br label %ifcont53

ifcont53:                                         ; preds = %else52, %cont74
  %iftmp86 = phi ptr [ %array_ptr_phi81, %cont74 ], [ 0.000000e+00, %else52 ]
  br label %for.step30

bounds.fail62:                                    ; preds = %then51
  %print_err64 = call i32 (ptr, ...) @printf(ptr @err_msg.3)
  ret ptr null

bounds.ok63:                                      ; preds = %then51
  %offset_idx65 = add i64 %__where_i_load56, 2
  %elem_ptr66 = getelementptr i64, ptr %__where_src_load55, i64 %offset_idx65
  %raw_val67 = load i64, ptr %elem_ptr66, align 8
  %len_ptr68 = getelementptr i64, ptr %__where_result_load54, i64 0
  %length69 = load i64, ptr %len_ptr68, align 8
  %cap_ptr70 = getelementptr i64, ptr %__where_result_load54, i64 1
  %capacity71 = load i64, ptr %cap_ptr70, align 8
  %is_full72 = icmp eq i64 %length69, %capacity71
  br i1 %is_full72, label %grow73, label %cont74

grow73:                                           ; preds = %bounds.ok63
  %new_capacity75 = mul i64 %capacity71, 2
  %1 = icmp eq i64 %capacity71, 0
  %new_capacity76 = select i1 %1, i64 4, i64 %new_capacity75
  %slots77 = add i64 %new_capacity76, 2
  %total_bytes78 = mul i64 %slots77, 8
  %realloc_array79 = call ptr @realloc(ptr %__where_result_load54, i64 %total_bytes78)
  %cap_ptr280 = getelementptr i64, ptr %realloc_array79, i64 1
  store i64 %new_capacity76, ptr %cap_ptr280, align 8
  br label %cont74

cont74:                                           ; preds = %grow73, %bounds.ok63
  %array_ptr_phi81 = phi ptr [ %__where_result_load54, %bounds.ok63 ], [ %realloc_array79, %grow73 ]
  %elem_index82 = add i64 %length69, 2
  %elem_ptr83 = getelementptr i64, ptr %array_ptr_phi81, i64 %elem_index82
  store i64 %raw_val67, ptr %elem_ptr83, align 8
  %new_length84 = add i64 %length69, 1
  %len_ptr285 = getelementptr i64, ptr %array_ptr_phi81, i64 0
  store i64 %new_length84, ptr %len_ptr285, align 8
  store ptr %array_ptr_phi81, ptr @__where_result, align 8
  br label %ifcont53
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
