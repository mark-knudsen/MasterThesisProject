; ModuleID = 'repl_module'
source_filename = "repl_module"

@arr = external global ptr
@__where_src = external global ptr, align 8
@__where_result = external global ptr, align 8
@__where_i = external global i64, align 8
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.3 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_108() {
entry:
  %arr_load = load ptr, ptr @arr, align 8
  store ptr %arr_load, ptr @__where_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 800)
  %arr_data = bitcast ptr %arr_data_raw to ptr
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %len_ptr, align 8
  store i64 100, ptr %cap_ptr, align 8
  store ptr %arr_data, ptr %data_field_ptr, align 8
  store ptr %arr_header, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__where_i_load = load i64, ptr @__where_i, align 8
  %__where_src_load = load ptr, ptr @__where_src, align 8
  %length_ptr = getelementptr i64, ptr %__where_src_load, i64 0
  %length = load i64, ptr %length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__where_i_load, %length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__where_src_load1 = load ptr, ptr @__where_src, align 8
  %__where_i_load2 = load i64, ptr @__where_i, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load1, i32 0, i32 0
  %data_field_ptr3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load1, i32 0, i32 2
  %array_len = load i64, ptr %len_field_ptr, align 8
  %data_ptr = load ptr, ptr %data_field_ptr3, align 8
  %0 = icmp slt i64 %__where_i_load2, 0
  %1 = icmp sge i64 %__where_i_load2, %array_len
  %is_invalid = or i1 %0, %1
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %ifcont
  %x_load = load i64, ptr @__where_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__where_result_load19 = load ptr, ptr @__where_result, align 8
  store ptr %__where_result_load19, ptr @__where_src, align 8
  %arr_header20 = call ptr @malloc(i64 24)
  %arr_data_raw21 = call ptr @malloc(i64 800)
  %arr_data22 = bitcast ptr %arr_data_raw21 to ptr
  %len_ptr23 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header20, i32 0, i32 0
  %cap_ptr24 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header20, i32 0, i32 1
  %data_field_ptr25 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header20, i32 0, i32 2
  store i64 0, ptr %len_ptr23, align 8
  store i64 100, ptr %cap_ptr24, align 8
  store ptr %arr_data22, ptr %data_field_ptr25, align 8
  store ptr %arr_header20, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond26

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %elem_ptr = getelementptr ptr, ptr %data_ptr, i64 %__where_i_load2
  %raw_val = load ptr, ptr %elem_ptr, align 8
  %2 = ptrtoint ptr %raw_val to i64
  %icmp_tmp4 = icmp sge i64 %2, 1000
  br i1 %icmp_tmp4, label %then, label %else

then:                                             ; preds = %bounds.ok
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load5 = load ptr, ptr @__where_src, align 8
  %__where_i_load6 = load i64, ptr @__where_i, align 8
  %len_field_ptr7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load5, i32 0, i32 0
  %data_field_ptr8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load5, i32 0, i32 2
  %array_len9 = load i64, ptr %len_field_ptr7, align 8
  %data_ptr10 = load ptr, ptr %data_field_ptr8, align 8
  %3 = icmp slt i64 %__where_i_load6, 0
  %4 = icmp sge i64 %__where_i_load6, %array_len9
  %is_invalid11 = or i1 %3, %4
  br i1 %is_invalid11, label %bounds.fail12, label %bounds.ok13

else:                                             ; preds = %bounds.ok
  br label %ifcont

ifcont:                                           ; preds = %else, %cont
  %iftmp = phi ptr [ %__where_result_load, %cont ], [ 0.000000e+00, %else ]
  br label %for.step

bounds.fail12:                                    ; preds = %then
  %print_err14 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok13:                                      ; preds = %then
  %elem_ptr15 = getelementptr ptr, ptr %data_ptr10, i64 %__where_i_load6
  %raw_val16 = load ptr, ptr %elem_ptr15, align 8
  %5 = ptrtoint ptr %raw_val16 to i64
  %len_ptr17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load, i32 0, i32 0
  %cap_ptr18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load, i32 0, i32 2
  %len = load i64, ptr %len_ptr17, align 8
  %cap = load i64, ptr %cap_ptr18, align 8
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %bounds.ok13
  %6 = icmp eq i64 %cap, 0
  %7 = mul i64 %cap, 2
  %new_cap = select i1 %6, i64 4, i64 %7
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr18, align 8
  store ptr %realloc, ptr %data_ptr_ptr, align 8
  br label %cont

cont:                                             ; preds = %grow, %bounds.ok13
  %final_data_ptr = phi ptr [ %data, %bounds.ok13 ], [ %realloc, %grow ]
  %target = getelementptr i64, ptr %final_data_ptr, i64 %len
  store i64 %5, ptr %target, align 4
  %new_len = add i64 %len, 1
  store i64 %new_len, ptr %len_ptr17, align 8
  br label %ifcont

for.cond26:                                       ; preds = %for.step28, %for.end
  %__where_i_load30 = load i64, ptr @__where_i, align 8
  %__where_src_load31 = load ptr, ptr @__where_src, align 8
  %length_ptr32 = getelementptr i64, ptr %__where_src_load31, i64 0
  %length33 = load i64, ptr %length_ptr32, align 8
  %icmp_tmp34 = icmp slt i64 %__where_i_load30, %length33
  br i1 %icmp_tmp34, label %for.body27, label %for.end29

for.body27:                                       ; preds = %for.cond26
  %__where_src_load35 = load ptr, ptr @__where_src, align 8
  %__where_i_load36 = load i64, ptr @__where_i, align 8
  %len_field_ptr37 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load35, i32 0, i32 0
  %data_field_ptr38 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load35, i32 0, i32 2
  %array_len39 = load i64, ptr %len_field_ptr37, align 8
  %data_ptr40 = load ptr, ptr %data_field_ptr38, align 8
  %8 = icmp slt i64 %__where_i_load36, 0
  %9 = icmp sge i64 %__where_i_load36, %array_len39
  %is_invalid41 = or i1 %8, %9
  br i1 %is_invalid41, label %bounds.fail42, label %bounds.ok43

for.step28:                                       ; preds = %ifcont50
  %x_load80 = load i64, ptr @__where_i, align 8
  %inc_add81 = add i64 %x_load80, 1
  store i64 %inc_add81, ptr @__where_i, align 8
  br label %for.cond26

for.end29:                                        ; preds = %for.cond26
  %__where_result_load82 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 5, ptr %tag_ptr, align 8
  %data_ptr83 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__where_result_load82, ptr %data_ptr83, align 8
  ret ptr %runtime_obj

bounds.fail42:                                    ; preds = %for.body27
  %print_err44 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok43:                                      ; preds = %for.body27
  %elem_ptr45 = getelementptr ptr, ptr %data_ptr40, i64 %__where_i_load36
  %raw_val46 = load ptr, ptr %elem_ptr45, align 8
  %10 = ptrtoint ptr %raw_val46 to i64
  %icmp_tmp47 = icmp slt i64 %10, 500000
  br i1 %icmp_tmp47, label %then48, label %else49

then48:                                           ; preds = %bounds.ok43
  %__where_result_load51 = load ptr, ptr @__where_result, align 8
  %__where_src_load52 = load ptr, ptr @__where_src, align 8
  %__where_i_load53 = load i64, ptr @__where_i, align 8
  %len_field_ptr54 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load52, i32 0, i32 0
  %data_field_ptr55 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_src_load52, i32 0, i32 2
  %array_len56 = load i64, ptr %len_field_ptr54, align 8
  %data_ptr57 = load ptr, ptr %data_field_ptr55, align 8
  %11 = icmp slt i64 %__where_i_load53, 0
  %12 = icmp sge i64 %__where_i_load53, %array_len56
  %is_invalid58 = or i1 %11, %12
  br i1 %is_invalid58, label %bounds.fail59, label %bounds.ok60

else49:                                           ; preds = %bounds.ok43
  br label %ifcont50

ifcont50:                                         ; preds = %else49, %cont72
  %iftmp79 = phi ptr [ %__where_result_load51, %cont72 ], [ 0.000000e+00, %else49 ]
  br label %for.step28

bounds.fail59:                                    ; preds = %then48
  %print_err61 = call i32 (ptr, ...) @printf(ptr @err_msg.3)
  ret ptr null

bounds.ok60:                                      ; preds = %then48
  %elem_ptr62 = getelementptr ptr, ptr %data_ptr57, i64 %__where_i_load53
  %raw_val63 = load ptr, ptr %elem_ptr62, align 8
  %13 = ptrtoint ptr %raw_val63 to i64
  %len_ptr64 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load51, i32 0, i32 0
  %cap_ptr65 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load51, i32 0, i32 1
  %data_ptr_ptr66 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_result_load51, i32 0, i32 2
  %len67 = load i64, ptr %len_ptr64, align 8
  %cap68 = load i64, ptr %cap_ptr65, align 8
  %data69 = load ptr, ptr %data_ptr_ptr66, align 8
  %is_full70 = icmp uge i64 %len67, %cap68
  br i1 %is_full70, label %grow71, label %cont72

grow71:                                           ; preds = %bounds.ok60
  %14 = icmp eq i64 %cap68, 0
  %15 = mul i64 %cap68, 2
  %new_cap73 = select i1 %14, i64 4, i64 %15
  %bytes74 = mul i64 %new_cap73, 8
  %realloc75 = call ptr @realloc(ptr %data69, i64 %bytes74)
  store i64 %new_cap73, ptr %cap_ptr65, align 8
  store ptr %realloc75, ptr %data_ptr_ptr66, align 8
  br label %cont72

cont72:                                           ; preds = %grow71, %bounds.ok60
  %final_data_ptr76 = phi ptr [ %data69, %bounds.ok60 ], [ %realloc75, %grow71 ]
  %target77 = getelementptr i64, ptr %final_data_ptr76, i64 %len67
  store i64 %13, ptr %target77, align 4
  %new_len78 = add i64 %len67, 1
  store i64 %new_len78, ptr %len_ptr64, align 8
  br label %ifcont50
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
