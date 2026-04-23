; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = global ptr null, align 8
@y = global ptr null, align 8
@n = global i64 0, align 8
@sum_x = global i64 0, align 8
@sum_y = global i64 0, align 8
@i = global i64 0, align 8
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@mean_x = global double 0.000000e+00, align 8
@mean_y = global double 0.000000e+00, align 8
@num = global i64 0, align 8
@denom_x = global i64 0, align 8
@denom_y = global i64 0, align 8
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@dx = global double 0.000000e+00, align 8
@err_msg.3 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@dy = global double 0.000000e+00, align 8
@corr = global double 0.000000e+00, align 8
@fmt_float_raw = private unnamed_addr constant [4 x i8] c"%f\0A\00", align 1

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
  store ptr %arr_header, ptr @x, align 8
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
  store ptr %arr_header4, ptr @y, align 8
  %x_load = load ptr, ptr @x, align 8
  %length_ptr = getelementptr i64, ptr %x_load, i64 0
  %length = load i64, ptr %length_ptr, align 4
  store i64 %length, ptr @n, align 8
  store i64 0, ptr @sum_x, align 8
  store i64 0, ptr @sum_y, align 8
  store i64 0, ptr @i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %i_load = load i64, ptr @i, align 8
  %n_load = load i64, ptr @n, align 8
  %icmp_tmp = icmp slt i64 %i_load, %n_load
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %sum_x_load = load i64, ptr @sum_x, align 8
  %x_load13 = load ptr, ptr @x, align 8
  %i_load14 = load i64, ptr @i, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load13, i32 0, i32 0
  %data_field_ptr15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load13, i32 0, i32 2
  %array_len = load i64, ptr %len_field_ptr, align 4
  %data_ptr = load ptr, ptr %data_field_ptr15, align 8
  %0 = icmp slt i64 %i_load14, 0
  %1 = icmp sge i64 %i_load14, %array_len
  %is_invalid = or i1 %0, %1
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %bounds.ok24
  %x_load29 = load i64, ptr @i, align 8
  %inc_add = add i64 %x_load29, 1
  store i64 %inc_add, ptr @i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %sum_x_load30 = load i64, ptr @sum_x, align 8
  %int2double = sitofp i64 %sum_x_load30 to double
  %n_load31 = load i64, ptr @n, align 8
  %int2double32 = sitofp i64 %n_load31 to double
  %fdivtmp = fdiv double %int2double, %int2double32
  store double %fdivtmp, ptr @mean_x, align 8
  %sum_y_load33 = load i64, ptr @sum_y, align 8
  %int2double34 = sitofp i64 %sum_y_load33 to double
  %n_load35 = load i64, ptr @n, align 8
  %int2double36 = sitofp i64 %n_load35 to double
  %fdivtmp37 = fdiv double %int2double34, %int2double36
  store double %fdivtmp37, ptr @mean_y, align 8
  store i64 0, ptr @num, align 8
  store i64 0, ptr @denom_x, align 8
  store i64 0, ptr @denom_y, align 8
  store i64 0, ptr @i, align 8
  br label %for.cond38

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %elem_ptr16 = getelementptr ptr, ptr %data_ptr, i64 %i_load14
  %raw_val = load ptr, ptr %elem_ptr16, align 8
  %2 = ptrtoint ptr %raw_val to i64
  %addtmp = add i64 %sum_x_load, %2
  store i64 %addtmp, ptr @sum_x, align 8
  %sum_y_load = load i64, ptr @sum_y, align 8
  %y_load = load ptr, ptr @y, align 8
  %i_load17 = load i64, ptr @i, align 8
  %len_field_ptr18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %y_load, i32 0, i32 0
  %data_field_ptr19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %y_load, i32 0, i32 2
  %array_len20 = load i64, ptr %len_field_ptr18, align 4
  %data_ptr21 = load ptr, ptr %data_field_ptr19, align 8
  %3 = icmp slt i64 %i_load17, 0
  %4 = icmp sge i64 %i_load17, %array_len20
  %is_invalid22 = or i1 %3, %4
  br i1 %is_invalid22, label %bounds.fail23, label %bounds.ok24

bounds.fail23:                                    ; preds = %bounds.ok
  %print_err25 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok24:                                      ; preds = %bounds.ok
  %elem_ptr26 = getelementptr ptr, ptr %data_ptr21, i64 %i_load17
  %raw_val27 = load ptr, ptr %elem_ptr26, align 8
  %5 = ptrtoint ptr %raw_val27 to i64
  %addtmp28 = add i64 %sum_y_load, %5
  store i64 %addtmp28, ptr @sum_y, align 8
  br label %for.step

for.cond38:                                       ; preds = %for.step40, %for.end
  %i_load42 = load i64, ptr @i, align 8
  %n_load43 = load i64, ptr @n, align 8
  %icmp_tmp44 = icmp slt i64 %i_load42, %n_load43
  br i1 %icmp_tmp44, label %for.body39, label %for.end41

for.body39:                                       ; preds = %for.cond38
  %x_load45 = load ptr, ptr @x, align 8
  %i_load46 = load i64, ptr @i, align 8
  %len_field_ptr47 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load45, i32 0, i32 0
  %data_field_ptr48 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load45, i32 0, i32 2
  %array_len49 = load i64, ptr %len_field_ptr47, align 4
  %data_ptr50 = load ptr, ptr %data_field_ptr48, align 8
  %6 = icmp slt i64 %i_load46, 0
  %7 = icmp sge i64 %i_load46, %array_len49
  %is_invalid51 = or i1 %6, %7
  br i1 %is_invalid51, label %bounds.fail52, label %bounds.ok53

for.step40:                                       ; preds = %bounds.ok66
  %x_load80 = load i64, ptr @i, align 8
  %inc_add81 = add i64 %x_load80, 1
  store i64 %inc_add81, ptr @i, align 8
  br label %for.cond38

for.end41:                                        ; preds = %for.cond38
  %denom_x_load82 = load double, ptr @denom_x, align 8
  %denom_y_load83 = load double, ptr @denom_y, align 8
  %fmultmp84 = fmul double %denom_x_load82, %denom_y_load83
  store double %fmultmp84, ptr @corr, align 8
  %corr_load = load double, ptr @corr, align 8
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_float_raw, double %corr_load)
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr85 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr85, align 8
  ret ptr %runtime_obj

bounds.fail52:                                    ; preds = %for.body39
  %print_err54 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok53:                                      ; preds = %for.body39
  %elem_ptr55 = getelementptr ptr, ptr %data_ptr50, i64 %i_load46
  %raw_val56 = load ptr, ptr %elem_ptr55, align 8
  %8 = ptrtoint ptr %raw_val56 to i64
  %mean_x_load = load double, ptr @mean_x, align 8
  %int2double57 = sitofp i64 %8 to double
  %fsubtmp = fsub double %int2double57, %mean_x_load
  store double %fsubtmp, ptr @dx, align 8
  %y_load58 = load ptr, ptr @y, align 8
  %i_load59 = load i64, ptr @i, align 8
  %len_field_ptr60 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %y_load58, i32 0, i32 0
  %data_field_ptr61 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %y_load58, i32 0, i32 2
  %array_len62 = load i64, ptr %len_field_ptr60, align 4
  %data_ptr63 = load ptr, ptr %data_field_ptr61, align 8
  %9 = icmp slt i64 %i_load59, 0
  %10 = icmp sge i64 %i_load59, %array_len62
  %is_invalid64 = or i1 %9, %10
  br i1 %is_invalid64, label %bounds.fail65, label %bounds.ok66

bounds.fail65:                                    ; preds = %bounds.ok53
  %print_err67 = call i32 (ptr, ...) @printf(ptr @err_msg.3)
  ret ptr null

bounds.ok66:                                      ; preds = %bounds.ok53
  %elem_ptr68 = getelementptr ptr, ptr %data_ptr63, i64 %i_load59
  %raw_val69 = load ptr, ptr %elem_ptr68, align 8
  %11 = ptrtoint ptr %raw_val69 to i64
  %mean_y_load = load double, ptr @mean_y, align 8
  %int2double70 = sitofp i64 %11 to double
  %fsubtmp71 = fsub double %int2double70, %mean_y_load
  store double %fsubtmp71, ptr @dy, align 8
  %num_load = load double, ptr @num, align 8
  %dx_load = load double, ptr @dx, align 8
  %dy_load = load double, ptr @dy, align 8
  %fmultmp = fmul double %dx_load, %dy_load
  %faddtmp = fadd double %num_load, %fmultmp
  store double %faddtmp, ptr @num, align 8
  %denom_x_load = load double, ptr @denom_x, align 8
  %dx_load72 = load double, ptr @dx, align 8
  %dx_load73 = load double, ptr @dx, align 8
  %fmultmp74 = fmul double %dx_load72, %dx_load73
  %faddtmp75 = fadd double %denom_x_load, %fmultmp74
  store double %faddtmp75, ptr @denom_x, align 8
  %denom_y_load = load double, ptr @denom_y, align 8
  %dy_load76 = load double, ptr @dy, align 8
  %dy_load77 = load double, ptr @dy, align 8
  %fmultmp78 = fmul double %dy_load76, %dy_load77
  %faddtmp79 = fadd double %denom_y_load, %fmultmp78
  store double %faddtmp79, ptr @denom_y, align 8
  br label %for.step40
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
