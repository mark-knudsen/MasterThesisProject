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
@mean_x = global i64 0, align 8
@mean_y = global i64 0, align 8
@num = global double 0.000000e+00, align 8
@denom_x = global double 0.000000e+00, align 8
@denom_y = global double 0.000000e+00, align 8
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@dx = global i64 0, align 8
@err_msg.3 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@dy = global i64 0, align 8
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
  %n_load31 = load i64, ptr @n, align 8
  %divtmp = sdiv i64 %sum_x_load30, %n_load31
  store i64 %divtmp, ptr @mean_x, align 8
  %sum_y_load32 = load i64, ptr @sum_y, align 8
  %n_load33 = load i64, ptr @n, align 8
  %divtmp34 = sdiv i64 %sum_y_load32, %n_load33
  store i64 %divtmp34, ptr @mean_y, align 8
  store double 0.000000e+00, ptr @num, align 8
  store double 0.000000e+00, ptr @denom_x, align 8
  store double 0.000000e+00, ptr @denom_y, align 8
  store i64 0, ptr @i, align 8
  br label %for.cond35

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

for.cond35:                                       ; preds = %for.step37, %for.end
  %i_load39 = load i64, ptr @i, align 8
  %n_load40 = load i64, ptr @n, align 8
  %icmp_tmp41 = icmp slt i64 %i_load39, %n_load40
  br i1 %icmp_tmp41, label %for.body36, label %for.end38

for.body36:                                       ; preds = %for.cond35
  %x_load42 = load ptr, ptr @x, align 8
  %i_load43 = load i64, ptr @i, align 8
  %len_field_ptr44 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load42, i32 0, i32 0
  %data_field_ptr45 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %x_load42, i32 0, i32 2
  %array_len46 = load i64, ptr %len_field_ptr44, align 4
  %data_ptr47 = load ptr, ptr %data_field_ptr45, align 8
  %6 = icmp slt i64 %i_load43, 0
  %7 = icmp sge i64 %i_load43, %array_len46
  %is_invalid48 = or i1 %6, %7
  br i1 %is_invalid48, label %bounds.fail49, label %bounds.ok50

for.step37:                                       ; preds = %bounds.ok62
  %x_load77 = load i64, ptr @i, align 8
  %inc_add78 = add i64 %x_load77, 1
  store i64 %inc_add78, ptr @i, align 8
  br label %for.cond35

for.end38:                                        ; preds = %for.cond35
  %denom_x_load79 = load double, ptr @denom_x, align 8
  %denom_y_load80 = load double, ptr @denom_y, align 8
  %fmultmp = fmul double %denom_x_load79, %denom_y_load80
  store double %fmultmp, ptr @corr, align 8
  %corr_load = load double, ptr @corr, align 8
  %printf_call = call i32 (ptr, ...) @printf(ptr @fmt_float_raw, double %corr_load)
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr81 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr81, align 8
  ret ptr %runtime_obj

bounds.fail49:                                    ; preds = %for.body36
  %print_err51 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok50:                                      ; preds = %for.body36
  %elem_ptr52 = getelementptr ptr, ptr %data_ptr47, i64 %i_load43
  %raw_val53 = load ptr, ptr %elem_ptr52, align 8
  %8 = ptrtoint ptr %raw_val53 to i64
  %mean_x_load = load i64, ptr @mean_x, align 8
  %subtmp = sub i64 %8, %mean_x_load
  store i64 %subtmp, ptr @dx, align 8
  %y_load54 = load ptr, ptr @y, align 8
  %i_load55 = load i64, ptr @i, align 8
  %len_field_ptr56 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %y_load54, i32 0, i32 0
  %data_field_ptr57 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %y_load54, i32 0, i32 2
  %array_len58 = load i64, ptr %len_field_ptr56, align 4
  %data_ptr59 = load ptr, ptr %data_field_ptr57, align 8
  %9 = icmp slt i64 %i_load55, 0
  %10 = icmp sge i64 %i_load55, %array_len58
  %is_invalid60 = or i1 %9, %10
  br i1 %is_invalid60, label %bounds.fail61, label %bounds.ok62

bounds.fail61:                                    ; preds = %bounds.ok50
  %print_err63 = call i32 (ptr, ...) @printf(ptr @err_msg.3)
  ret ptr null

bounds.ok62:                                      ; preds = %bounds.ok50
  %elem_ptr64 = getelementptr ptr, ptr %data_ptr59, i64 %i_load55
  %raw_val65 = load ptr, ptr %elem_ptr64, align 8
  %11 = ptrtoint ptr %raw_val65 to i64
  %mean_y_load = load i64, ptr @mean_y, align 8
  %subtmp66 = sub i64 %11, %mean_y_load
  store i64 %subtmp66, ptr @dy, align 8
  %num_load = load double, ptr @num, align 8
  %dx_load = load i64, ptr @dx, align 8
  %dy_load = load i64, ptr @dy, align 8
  %multmp = mul i64 %dx_load, %dy_load
  %int2double = sitofp i64 %multmp to double
  %faddtmp = fadd double %num_load, %int2double
  store double %faddtmp, ptr @num, align 8
  %denom_x_load = load double, ptr @denom_x, align 8
  %dx_load67 = load i64, ptr @dx, align 8
  %dx_load68 = load i64, ptr @dx, align 8
  %multmp69 = mul i64 %dx_load67, %dx_load68
  %int2double70 = sitofp i64 %multmp69 to double
  %faddtmp71 = fadd double %denom_x_load, %int2double70
  store double %faddtmp71, ptr @denom_x, align 8
  %denom_y_load = load double, ptr @denom_y, align 8
  %dy_load72 = load i64, ptr @dy, align 8
  %dy_load73 = load i64, ptr @dy, align 8
  %multmp74 = mul i64 %dy_load72, %dy_load73
  %int2double75 = sitofp i64 %multmp74 to double
  %faddtmp76 = fadd double %denom_y_load, %int2double75
  store double %faddtmp76, ptr @denom_y, align 8
  br label %for.step37
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
