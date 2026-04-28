; ModuleID = 'repl_module'
source_filename = "repl_module"

@x = external global ptr
@__corr_x = external global ptr, align 8
@y = external global ptr
@__corr_y = external global ptr, align 8
@__corr_sum_x = external global i64, align 8
@__corr_sum_y = external global i64, align 8
@__corr_i = external global i64, align 8
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@__corr_mean_x = external global double, align 8
@__corr_mean_y = external global double, align 8
@__corr_num = external global i64, align 8
@__corr_dx = external global i64, align 8
@__corr_dy = external global i64, align 8
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.3 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.4 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.5 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.6 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.7 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@__corr_result = external global double, align 8

define ptr @main_8() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__corr_x, align 8
  %y_load = load ptr, ptr @y, align 8
  store ptr %y_load, ptr @__corr_y, align 8
  store i64 0, ptr @__corr_sum_x, align 8
  store i64 0, ptr @__corr_sum_y, align 8
  store i64 0, ptr @__corr_i, align 8
  store i64 0, ptr @__corr_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__corr_i_load = load i64, ptr @__corr_i, align 8
  %__corr_x_load = load ptr, ptr @__corr_x, align 8
  %length_ptr = getelementptr i64, ptr %__corr_x_load, i64 0
  %length = load i64, ptr %length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__corr_i_load, %length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__corr_sum_x_load = load i64, ptr @__corr_sum_x, align 8
  %__corr_x_load1 = load ptr, ptr @__corr_x, align 8
  %__corr_i_load2 = load i64, ptr @__corr_i, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load1, i32 0, i32 0
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load1, i32 0, i32 2
  %array_len = load i64, ptr %len_field_ptr, align 8
  %data_ptr = load ptr, ptr %data_field_ptr, align 8
  %0 = icmp slt i64 %__corr_i_load2, 0
  %1 = icmp sge i64 %__corr_i_load2, %array_len
  %is_invalid = or i1 %0, %1
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %bounds.ok10
  %x_load15 = load i64, ptr @__corr_i, align 8
  %inc_add = add i64 %x_load15, 1
  store i64 %inc_add, ptr @__corr_i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__corr_sum_x_load16 = load i64, ptr @__corr_sum_x, align 8
  %int2double = sitofp i64 %__corr_sum_x_load16 to double
  %__corr_x_load17 = load ptr, ptr @__corr_x, align 8
  %length_ptr18 = getelementptr i64, ptr %__corr_x_load17, i64 0
  %length19 = load i64, ptr %length_ptr18, align 8
  %int2double20 = sitofp i64 %length19 to double
  %fdivtmp = fdiv double %int2double, %int2double20
  store double %fdivtmp, ptr @__corr_mean_x, align 8
  %__corr_sum_y_load21 = load i64, ptr @__corr_sum_y, align 8
  %int2double22 = sitofp i64 %__corr_sum_y_load21 to double
  %__corr_x_load23 = load ptr, ptr @__corr_x, align 8
  %length_ptr24 = getelementptr i64, ptr %__corr_x_load23, i64 0
  %length25 = load i64, ptr %length_ptr24, align 8
  %int2double26 = sitofp i64 %length25 to double
  %fdivtmp27 = fdiv double %int2double22, %int2double26
  store double %fdivtmp27, ptr @__corr_mean_y, align 8
  store i64 0, ptr @__corr_i, align 8
  store i64 0, ptr @__corr_num, align 8
  store i64 0, ptr @__corr_dx, align 8
  store i64 0, ptr @__corr_dy, align 8
  store i64 0, ptr @__corr_i, align 8
  br label %for.cond28

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %elem_ptr = getelementptr ptr, ptr %data_ptr, i64 %__corr_i_load2
  %raw_val = load ptr, ptr %elem_ptr, align 8
  %2 = ptrtoint ptr %raw_val to i64
  %addtmp = add i64 %__corr_sum_x_load, %2
  store i64 %addtmp, ptr @__corr_sum_x, align 8
  %__corr_sum_y_load = load i64, ptr @__corr_sum_y, align 8
  %__corr_y_load = load ptr, ptr @__corr_y, align 8
  %__corr_i_load3 = load i64, ptr @__corr_i, align 8
  %len_field_ptr4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load, i32 0, i32 0
  %data_field_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load, i32 0, i32 2
  %array_len6 = load i64, ptr %len_field_ptr4, align 8
  %data_ptr7 = load ptr, ptr %data_field_ptr5, align 8
  %3 = icmp slt i64 %__corr_i_load3, 0
  %4 = icmp sge i64 %__corr_i_load3, %array_len6
  %is_invalid8 = or i1 %3, %4
  br i1 %is_invalid8, label %bounds.fail9, label %bounds.ok10

bounds.fail9:                                     ; preds = %bounds.ok
  %print_err11 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok10:                                      ; preds = %bounds.ok
  %elem_ptr12 = getelementptr ptr, ptr %data_ptr7, i64 %__corr_i_load3
  %raw_val13 = load ptr, ptr %elem_ptr12, align 8
  %5 = ptrtoint ptr %raw_val13 to i64
  %addtmp14 = add i64 %__corr_sum_y_load, %5
  store i64 %addtmp14, ptr @__corr_sum_y, align 8
  br label %for.step

for.cond28:                                       ; preds = %for.step30, %for.end
  %__corr_i_load32 = load i64, ptr @__corr_i, align 8
  %__corr_x_load33 = load ptr, ptr @__corr_x, align 8
  %length_ptr34 = getelementptr i64, ptr %__corr_x_load33, i64 0
  %length35 = load i64, ptr %length_ptr34, align 8
  %icmp_tmp36 = icmp slt i64 %__corr_i_load32, %length35
  br i1 %icmp_tmp36, label %for.body29, label %for.end31

for.body29:                                       ; preds = %for.cond28
  %__corr_num_load = load double, ptr @__corr_num, align 8
  %__corr_x_load37 = load ptr, ptr @__corr_x, align 8
  %__corr_i_load38 = load i64, ptr @__corr_i, align 8
  %len_field_ptr39 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load37, i32 0, i32 0
  %data_field_ptr40 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load37, i32 0, i32 2
  %array_len41 = load i64, ptr %len_field_ptr39, align 8
  %data_ptr42 = load ptr, ptr %data_field_ptr40, align 8
  %6 = icmp slt i64 %__corr_i_load38, 0
  %7 = icmp sge i64 %__corr_i_load38, %array_len41
  %is_invalid43 = or i1 %6, %7
  br i1 %is_invalid43, label %bounds.fail44, label %bounds.ok45

for.step30:                                       ; preds = %bounds.ok119
  %x_load128 = load i64, ptr @__corr_i, align 8
  %inc_add129 = add i64 %x_load128, 1
  store i64 %inc_add129, ptr @__corr_i, align 8
  br label %for.cond28

for.end31:                                        ; preds = %for.cond28
  %__corr_num_load130 = load double, ptr @__corr_num, align 8
  %__corr_dx_load131 = load double, ptr @__corr_dx, align 8
  %__corr_dy_load132 = load double, ptr @__corr_dy, align 8
  %fmultmp133 = fmul double %__corr_dx_load131, %__corr_dy_load132
  %sqrttmp = call double @llvm.sqrt.f64(double %fmultmp133)
  %fdivtmp134 = fdiv double %__corr_num_load130, %sqrttmp
  store double %fdivtmp134, ptr @__corr_result, align 8
  %__corr_result_load = load double, ptr @__corr_result, align 8
  %value_mem = call ptr @malloc(i64 8)
  %value_cast = bitcast ptr %value_mem to ptr
  store double %__corr_result_load, ptr %value_cast, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 2, ptr %tag_ptr, align 8
  %data_ptr135 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr135, align 8
  ret ptr %runtime_obj

bounds.fail44:                                    ; preds = %for.body29
  %print_err46 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok45:                                      ; preds = %for.body29
  %elem_ptr47 = getelementptr ptr, ptr %data_ptr42, i64 %__corr_i_load38
  %raw_val48 = load ptr, ptr %elem_ptr47, align 8
  %8 = ptrtoint ptr %raw_val48 to i64
  %__corr_mean_x_load = load double, ptr @__corr_mean_x, align 8
  %int2double49 = sitofp i64 %8 to double
  %fsubtmp = fsub double %int2double49, %__corr_mean_x_load
  %__corr_y_load50 = load ptr, ptr @__corr_y, align 8
  %__corr_i_load51 = load i64, ptr @__corr_i, align 8
  %len_field_ptr52 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load50, i32 0, i32 0
  %data_field_ptr53 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load50, i32 0, i32 2
  %array_len54 = load i64, ptr %len_field_ptr52, align 8
  %data_ptr55 = load ptr, ptr %data_field_ptr53, align 8
  %9 = icmp slt i64 %__corr_i_load51, 0
  %10 = icmp sge i64 %__corr_i_load51, %array_len54
  %is_invalid56 = or i1 %9, %10
  br i1 %is_invalid56, label %bounds.fail57, label %bounds.ok58

bounds.fail57:                                    ; preds = %bounds.ok45
  %print_err59 = call i32 (ptr, ...) @printf(ptr @err_msg.3)
  ret ptr null

bounds.ok58:                                      ; preds = %bounds.ok45
  %elem_ptr60 = getelementptr ptr, ptr %data_ptr55, i64 %__corr_i_load51
  %raw_val61 = load ptr, ptr %elem_ptr60, align 8
  %11 = ptrtoint ptr %raw_val61 to i64
  %__corr_mean_y_load = load double, ptr @__corr_mean_y, align 8
  %int2double62 = sitofp i64 %11 to double
  %fsubtmp63 = fsub double %int2double62, %__corr_mean_y_load
  %fmultmp = fmul double %fsubtmp, %fsubtmp63
  %faddtmp = fadd double %__corr_num_load, %fmultmp
  store double %faddtmp, ptr @__corr_num, align 8
  %__corr_dx_load = load double, ptr @__corr_dx, align 8
  %__corr_x_load64 = load ptr, ptr @__corr_x, align 8
  %__corr_i_load65 = load i64, ptr @__corr_i, align 8
  %len_field_ptr66 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load64, i32 0, i32 0
  %data_field_ptr67 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load64, i32 0, i32 2
  %array_len68 = load i64, ptr %len_field_ptr66, align 8
  %data_ptr69 = load ptr, ptr %data_field_ptr67, align 8
  %12 = icmp slt i64 %__corr_i_load65, 0
  %13 = icmp sge i64 %__corr_i_load65, %array_len68
  %is_invalid70 = or i1 %12, %13
  br i1 %is_invalid70, label %bounds.fail71, label %bounds.ok72

bounds.fail71:                                    ; preds = %bounds.ok58
  %print_err73 = call i32 (ptr, ...) @printf(ptr @err_msg.4)
  ret ptr null

bounds.ok72:                                      ; preds = %bounds.ok58
  %elem_ptr74 = getelementptr ptr, ptr %data_ptr69, i64 %__corr_i_load65
  %raw_val75 = load ptr, ptr %elem_ptr74, align 8
  %14 = ptrtoint ptr %raw_val75 to i64
  %__corr_mean_x_load76 = load double, ptr @__corr_mean_x, align 8
  %int2double77 = sitofp i64 %14 to double
  %fsubtmp78 = fsub double %int2double77, %__corr_mean_x_load76
  %__corr_x_load79 = load ptr, ptr @__corr_x, align 8
  %__corr_i_load80 = load i64, ptr @__corr_i, align 8
  %len_field_ptr81 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load79, i32 0, i32 0
  %data_field_ptr82 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load79, i32 0, i32 2
  %array_len83 = load i64, ptr %len_field_ptr81, align 8
  %data_ptr84 = load ptr, ptr %data_field_ptr82, align 8
  %15 = icmp slt i64 %__corr_i_load80, 0
  %16 = icmp sge i64 %__corr_i_load80, %array_len83
  %is_invalid85 = or i1 %15, %16
  br i1 %is_invalid85, label %bounds.fail86, label %bounds.ok87

bounds.fail86:                                    ; preds = %bounds.ok72
  %print_err88 = call i32 (ptr, ...) @printf(ptr @err_msg.5)
  ret ptr null

bounds.ok87:                                      ; preds = %bounds.ok72
  %elem_ptr89 = getelementptr ptr, ptr %data_ptr84, i64 %__corr_i_load80
  %raw_val90 = load ptr, ptr %elem_ptr89, align 8
  %17 = ptrtoint ptr %raw_val90 to i64
  %__corr_mean_x_load91 = load double, ptr @__corr_mean_x, align 8
  %int2double92 = sitofp i64 %17 to double
  %fsubtmp93 = fsub double %int2double92, %__corr_mean_x_load91
  %fmultmp94 = fmul double %fsubtmp78, %fsubtmp93
  %faddtmp95 = fadd double %__corr_dx_load, %fmultmp94
  store double %faddtmp95, ptr @__corr_dx, align 8
  %__corr_dy_load = load double, ptr @__corr_dy, align 8
  %__corr_y_load96 = load ptr, ptr @__corr_y, align 8
  %__corr_i_load97 = load i64, ptr @__corr_i, align 8
  %len_field_ptr98 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load96, i32 0, i32 0
  %data_field_ptr99 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load96, i32 0, i32 2
  %array_len100 = load i64, ptr %len_field_ptr98, align 8
  %data_ptr101 = load ptr, ptr %data_field_ptr99, align 8
  %18 = icmp slt i64 %__corr_i_load97, 0
  %19 = icmp sge i64 %__corr_i_load97, %array_len100
  %is_invalid102 = or i1 %18, %19
  br i1 %is_invalid102, label %bounds.fail103, label %bounds.ok104

bounds.fail103:                                   ; preds = %bounds.ok87
  %print_err105 = call i32 (ptr, ...) @printf(ptr @err_msg.6)
  ret ptr null

bounds.ok104:                                     ; preds = %bounds.ok87
  %elem_ptr106 = getelementptr ptr, ptr %data_ptr101, i64 %__corr_i_load97
  %raw_val107 = load ptr, ptr %elem_ptr106, align 8
  %20 = ptrtoint ptr %raw_val107 to i64
  %__corr_mean_y_load108 = load double, ptr @__corr_mean_y, align 8
  %int2double109 = sitofp i64 %20 to double
  %fsubtmp110 = fsub double %int2double109, %__corr_mean_y_load108
  %__corr_y_load111 = load ptr, ptr @__corr_y, align 8
  %__corr_i_load112 = load i64, ptr @__corr_i, align 8
  %len_field_ptr113 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load111, i32 0, i32 0
  %data_field_ptr114 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load111, i32 0, i32 2
  %array_len115 = load i64, ptr %len_field_ptr113, align 8
  %data_ptr116 = load ptr, ptr %data_field_ptr114, align 8
  %21 = icmp slt i64 %__corr_i_load112, 0
  %22 = icmp sge i64 %__corr_i_load112, %array_len115
  %is_invalid117 = or i1 %21, %22
  br i1 %is_invalid117, label %bounds.fail118, label %bounds.ok119

bounds.fail118:                                   ; preds = %bounds.ok104
  %print_err120 = call i32 (ptr, ...) @printf(ptr @err_msg.7)
  ret ptr null

bounds.ok119:                                     ; preds = %bounds.ok104
  %elem_ptr121 = getelementptr ptr, ptr %data_ptr116, i64 %__corr_i_load112
  %raw_val122 = load ptr, ptr %elem_ptr121, align 8
  %23 = ptrtoint ptr %raw_val122 to i64
  %__corr_mean_y_load123 = load double, ptr @__corr_mean_y, align 8
  %int2double124 = sitofp i64 %23 to double
  %fsubtmp125 = fsub double %int2double124, %__corr_mean_y_load123
  %fmultmp126 = fmul double %fsubtmp110, %fsubtmp125
  %faddtmp127 = fadd double %__corr_dy_load, %fmultmp126
  store double %faddtmp127, ptr @__corr_dy, align 8
  br label %for.step30
}

declare i32 @printf(ptr, ...)

; Function Attrs: nocallback nofree nosync nounwind speculatable willreturn memory(none)
declare double @llvm.sqrt.f64(double) #0

declare ptr @malloc(i64)

attributes #0 = { nocallback nofree nosync nounwind speculatable willreturn memory(none) }
