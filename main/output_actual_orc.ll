; ModuleID = 'repl_module'
source_filename = "repl_module"

@xs = global ptr null, align 8
@ys = global ptr null, align 8
@__mean_array = global ptr null, align 8
@__mean_sum = global double 0.000000e+00, align 8
@__mean_i = global i64 0, align 8
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@__mean_val = global double 0.000000e+00, align 8
@mean_x = global double 0.000000e+00, align 8
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@mean_y = global double 0.000000e+00, align 8
@num = global i64 0, align 8
@den = global i64 0, align 8
@i = global i64 0, align 8
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@x = global i64 0, align 8
@err_msg.3 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@y = global i64 0, align 8
@slope = global double 0.000000e+00, align 8

define ptr @main_0() {
entry:
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 64)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 4, ptr %len_ptr, align 8
  store i64 8, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  %elem_ptr = getelementptr i64, ptr %arr_data_raw, i64 0
  store i64 1, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr i64, ptr %arr_data_raw, i64 1
  store i64 2, ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr i64, ptr %arr_data_raw, i64 2
  store i64 3, ptr %elem_ptr2, align 8
  %elem_ptr3 = getelementptr i64, ptr %arr_data_raw, i64 3
  store i64 4, ptr %elem_ptr3, align 8
  store ptr %arr_header, ptr @xs, align 8
  %arr_header4 = call ptr @malloc(i64 24)
  %arr_data_raw5 = call ptr @malloc(i64 64)
  %len_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 0
  %cap_ptr7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 1
  %data_field_ptr8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 2
  store i64 4, ptr %len_ptr6, align 8
  store i64 8, ptr %cap_ptr7, align 8
  store ptr %arr_data_raw5, ptr %data_field_ptr8, align 8
  %elem_ptr9 = getelementptr i64, ptr %arr_data_raw5, i64 0
  store i64 2, ptr %elem_ptr9, align 8
  %elem_ptr10 = getelementptr i64, ptr %arr_data_raw5, i64 1
  store i64 4, ptr %elem_ptr10, align 8
  %elem_ptr11 = getelementptr i64, ptr %arr_data_raw5, i64 2
  store i64 6, ptr %elem_ptr11, align 8
  %elem_ptr12 = getelementptr i64, ptr %arr_data_raw5, i64 3
  store i64 8, ptr %elem_ptr12, align 8
  store ptr %arr_header4, ptr @ys, align 8
  %xs_load = load ptr, ptr @xs, align 8
  store ptr %xs_load, ptr @__mean_array, align 8
  store double 0.000000e+00, ptr @__mean_sum, align 8
  store i64 0, ptr @__mean_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__mean_i_load = load i64, ptr @__mean_i, align 8
  %__mean_array_load = load ptr, ptr @__mean_array, align 8
  %length_ptr = getelementptr i64, ptr %__mean_array_load, i64 0
  %length = load i64, ptr %length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__mean_i_load, %length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__mean_sum_load = load double, ptr @__mean_sum, align 8
  %__mean_array_load13 = load ptr, ptr @__mean_array, align 8
  %__mean_i_load14 = load i64, ptr @__mean_i, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__mean_array_load13, i32 0, i32 0
  %data_field_ptr15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__mean_array_load13, i32 0, i32 2
  %array_len = load i64, ptr %len_field_ptr, align 8
  %data_ptr = load ptr, ptr %data_field_ptr15, align 8
  %0 = icmp slt i64 %__mean_i_load14, 0
  %1 = icmp sge i64 %__mean_i_load14, %array_len
  %is_invalid = or i1 %0, %1
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %bounds.ok
  %x_load = load i64, ptr @__mean_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__mean_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__mean_sum_load17 = load double, ptr @__mean_sum, align 8
  %__mean_array_load18 = load ptr, ptr @__mean_array, align 8
  %length_ptr19 = getelementptr i64, ptr %__mean_array_load18, i64 0
  %length20 = load i64, ptr %length_ptr19, align 8
  %int2double21 = sitofp i64 %length20 to double
  %fdivtmp = fdiv double %__mean_sum_load17, %int2double21
  store double %fdivtmp, ptr @__mean_val, align 8
  %__mean_val_load = load double, ptr @__mean_val, align 8
  store double %__mean_val_load, ptr @mean_x, align 8
  %ys_load = load ptr, ptr @ys, align 8
  store ptr %ys_load, ptr @__mean_array, align 8
  store double 0.000000e+00, ptr @__mean_sum, align 8
  store i64 0, ptr @__mean_i, align 8
  br label %for.cond22

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %elem_ptr16 = getelementptr ptr, ptr %data_ptr, i64 %__mean_i_load14
  %raw_val = load ptr, ptr %elem_ptr16, align 8
  %2 = ptrtoint ptr %raw_val to i64
  %int2double = sitofp i64 %2 to double
  %faddtmp = fadd double %__mean_sum_load, %int2double
  store double %faddtmp, ptr @__mean_sum, align 8
  br label %for.step

for.cond22:                                       ; preds = %for.step24, %for.end
  %__mean_i_load26 = load i64, ptr @__mean_i, align 8
  %__mean_array_load27 = load ptr, ptr @__mean_array, align 8
  %length_ptr28 = getelementptr i64, ptr %__mean_array_load27, i64 0
  %length29 = load i64, ptr %length_ptr28, align 8
  %icmp_tmp30 = icmp slt i64 %__mean_i_load26, %length29
  br i1 %icmp_tmp30, label %for.body23, label %for.end25

for.body23:                                       ; preds = %for.cond22
  %__mean_sum_load31 = load double, ptr @__mean_sum, align 8
  %__mean_array_load32 = load ptr, ptr @__mean_array, align 8
  %__mean_i_load33 = load i64, ptr @__mean_i, align 8
  %len_field_ptr34 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__mean_array_load32, i32 0, i32 0
  %data_field_ptr35 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__mean_array_load32, i32 0, i32 2
  %array_len36 = load i64, ptr %len_field_ptr34, align 8
  %data_ptr37 = load ptr, ptr %data_field_ptr35, align 8
  %3 = icmp slt i64 %__mean_i_load33, 0
  %4 = icmp sge i64 %__mean_i_load33, %array_len36
  %is_invalid38 = or i1 %3, %4
  br i1 %is_invalid38, label %bounds.fail39, label %bounds.ok40

for.step24:                                       ; preds = %bounds.ok40
  %x_load46 = load i64, ptr @__mean_i, align 8
  %inc_add47 = add i64 %x_load46, 1
  store i64 %inc_add47, ptr @__mean_i, align 8
  br label %for.cond22

for.end25:                                        ; preds = %for.cond22
  %__mean_sum_load48 = load double, ptr @__mean_sum, align 8
  %__mean_array_load49 = load ptr, ptr @__mean_array, align 8
  %length_ptr50 = getelementptr i64, ptr %__mean_array_load49, i64 0
  %length51 = load i64, ptr %length_ptr50, align 8
  %int2double52 = sitofp i64 %length51 to double
  %fdivtmp53 = fdiv double %__mean_sum_load48, %int2double52
  store double %fdivtmp53, ptr @__mean_val, align 8
  %__mean_val_load54 = load double, ptr @__mean_val, align 8
  store double %__mean_val_load54, ptr @mean_y, align 8
  store i64 0, ptr @num, align 8
  store i64 0, ptr @den, align 8
  store i64 0, ptr @i, align 8
  br label %for.cond55

bounds.fail39:                                    ; preds = %for.body23
  %print_err41 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok40:                                      ; preds = %for.body23
  %elem_ptr42 = getelementptr ptr, ptr %data_ptr37, i64 %__mean_i_load33
  %raw_val43 = load ptr, ptr %elem_ptr42, align 8
  %5 = ptrtoint ptr %raw_val43 to i64
  %int2double44 = sitofp i64 %5 to double
  %faddtmp45 = fadd double %__mean_sum_load31, %int2double44
  store double %faddtmp45, ptr @__mean_sum, align 8
  br label %for.step24

for.cond55:                                       ; preds = %for.step57, %for.end25
  %i_load = load i64, ptr @i, align 8
  %xs_load59 = load ptr, ptr @xs, align 8
  %length_ptr60 = getelementptr i64, ptr %xs_load59, i64 0
  %length61 = load i64, ptr %length_ptr60, align 8
  %icmp_tmp62 = icmp slt i64 %i_load, %length61
  br i1 %icmp_tmp62, label %for.body56, label %for.end58

for.body56:                                       ; preds = %for.cond55
  %xs_load63 = load ptr, ptr @xs, align 8
  %i_load64 = load i64, ptr @i, align 8
  %len_field_ptr65 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %xs_load63, i32 0, i32 0
  %data_field_ptr66 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %xs_load63, i32 0, i32 2
  %array_len67 = load i64, ptr %len_field_ptr65, align 8
  %data_ptr68 = load ptr, ptr %data_field_ptr66, align 8
  %6 = icmp slt i64 %i_load64, 0
  %7 = icmp sge i64 %i_load64, %array_len67
  %is_invalid69 = or i1 %6, %7
  br i1 %is_invalid69, label %bounds.fail70, label %bounds.ok71

for.step57:                                       ; preds = %bounds.ok83
  %x_load102 = load i64, ptr @i, align 8
  %inc_add103 = add i64 %x_load102, 1
  store i64 %inc_add103, ptr @i, align 8
  br label %for.cond55

for.end58:                                        ; preds = %for.cond55
  %num_load104 = load double, ptr @num, align 8
  %den_load105 = load double, ptr @den, align 8
  %fdivtmp106 = fdiv double %num_load104, %den_load105
  store double %fdivtmp106, ptr @slope, align 8
  %slope_load = load double, ptr @slope, align 8
  %value_mem = call ptr @malloc(i64 8)
  store double %slope_load, ptr %value_mem, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 2, ptr %tag_ptr, align 8
  %data_ptr107 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr107, align 8
  ret ptr %runtime_obj

bounds.fail70:                                    ; preds = %for.body56
  %print_err72 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok71:                                      ; preds = %for.body56
  %elem_ptr73 = getelementptr ptr, ptr %data_ptr68, i64 %i_load64
  %raw_val74 = load ptr, ptr %elem_ptr73, align 8
  %8 = ptrtoint ptr %raw_val74 to i64
  store i64 %8, ptr @x, align 8
  %ys_load75 = load ptr, ptr @ys, align 8
  %i_load76 = load i64, ptr @i, align 8
  %len_field_ptr77 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %ys_load75, i32 0, i32 0
  %data_field_ptr78 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %ys_load75, i32 0, i32 2
  %array_len79 = load i64, ptr %len_field_ptr77, align 8
  %data_ptr80 = load ptr, ptr %data_field_ptr78, align 8
  %9 = icmp slt i64 %i_load76, 0
  %10 = icmp sge i64 %i_load76, %array_len79
  %is_invalid81 = or i1 %9, %10
  br i1 %is_invalid81, label %bounds.fail82, label %bounds.ok83

bounds.fail82:                                    ; preds = %bounds.ok71
  %print_err84 = call i32 (ptr, ...) @printf(ptr @err_msg.3)
  ret ptr null

bounds.ok83:                                      ; preds = %bounds.ok71
  %elem_ptr85 = getelementptr ptr, ptr %data_ptr80, i64 %i_load76
  %raw_val86 = load ptr, ptr %elem_ptr85, align 8
  %11 = ptrtoint ptr %raw_val86 to i64
  store i64 %11, ptr @y, align 8
  %num_load = load double, ptr @num, align 8
  %x_load87 = load i64, ptr @x, align 8
  %mean_x_load = load double, ptr @mean_x, align 8
  %int2double88 = sitofp i64 %x_load87 to double
  %fsubtmp = fsub double %int2double88, %mean_x_load
  %y_load = load i64, ptr @y, align 8
  %mean_y_load = load double, ptr @mean_y, align 8
  %int2double89 = sitofp i64 %y_load to double
  %fsubtmp90 = fsub double %int2double89, %mean_y_load
  %fmultmp = fmul double %fsubtmp, %fsubtmp90
  %faddtmp91 = fadd double %num_load, %fmultmp
  store double %faddtmp91, ptr @num, align 8
  %den_load = load double, ptr @den, align 8
  %x_load92 = load i64, ptr @x, align 8
  %mean_x_load93 = load double, ptr @mean_x, align 8
  %int2double94 = sitofp i64 %x_load92 to double
  %fsubtmp95 = fsub double %int2double94, %mean_x_load93
  %x_load96 = load i64, ptr @x, align 8
  %mean_x_load97 = load double, ptr @mean_x, align 8
  %int2double98 = sitofp i64 %x_load96 to double
  %fsubtmp99 = fsub double %int2double98, %mean_x_load97
  %fmultmp100 = fmul double %fsubtmp95, %fsubtmp99
  %faddtmp101 = fadd double %den_load, %fmultmp100
  store double %faddtmp101, ptr @den, align 8
  br label %for.step57
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
