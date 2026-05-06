; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@df = external global ptr
@__col_src = external global ptr, align 8
@__col_len = external global i64, align 8
@__col_result = external global ptr, align 8
@__col_i = external global i64, align 8
@df_idx_err_msg = private unnamed_addr constant [50 x i8] c"Runtime Error: Dataframe row index out of bounds\0A\00", align 1
@__col_row = external global ptr, align 8
@__corr_x = external global ptr, align 8
@df_idx_err_msg.1 = private unnamed_addr constant [50 x i8] c"Runtime Error: Dataframe row index out of bounds\0A\00", align 1
@__corr_y = external global ptr, align 8
@__corr_sum_x = external global i64, align 8
@__corr_sum_y = external global i64, align 8
@__corr_i = external global i64, align 8
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@__corr_mean_x = external global double, align 8
@__corr_mean_y = external global double, align 8
@__corr_num = external global i64, align 8
@__corr_dx = external global i64, align 8
@__corr_dy = external global i64, align 8
@err_msg.3 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.4 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.5 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.6 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.7 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.8 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@__corr_result = external global double, align 8

define ptr @main_14() {
entry:
  %df_load = load ptr, ptr @df, align 8
  store ptr %df_load, ptr @__col_src, align 8
  %__col_src_load = load ptr, ptr @__col_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__col_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  store i64 %rows_length, ptr @__col_len, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 8)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %len_ptr, align 8
  store i64 0, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  store ptr %arr_header, ptr @__col_result, align 8
  store i64 0, ptr @__col_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__col_i_load = load i64, ptr @__col_i, align 8
  %__col_len_load = load i64, ptr @__col_len, align 8
  %icmp_tmp = icmp slt i64 %__col_i_load, %__col_len_load
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__col_src_load1 = load ptr, ptr @__col_src, align 8
  %__col_i_load2 = load i64, ptr @__col_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__col_src_load1, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %len_ptr3 = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr3, align 8
  %in_bounds = icmp ult i64 %__col_i_load2, %len
  br i1 %in_bounds, label %df_idx_ok, label %df_idx_err

for.step:                                         ; preds = %cont
  %x_load = load i64, ptr @__col_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__col_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__col_result_load9 = load ptr, ptr @__col_result, align 8
  store ptr %__col_result_load9, ptr @__corr_x, align 8
  %df_load10 = load ptr, ptr @df, align 8
  store ptr %df_load10, ptr @__col_src, align 8
  %__col_src_load11 = load ptr, ptr @__col_src, align 8
  %rows_ptr_field12 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__col_src_load11, i32 0, i32 1
  %rows_ptr13 = load ptr, ptr %rows_ptr_field12, align 8
  %rows_array_ptr14 = bitcast ptr %rows_ptr13 to ptr
  %rows_length_ptr15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr14, i32 0, i32 0
  %rows_length16 = load i64, ptr %rows_length_ptr15, align 8
  store i64 %rows_length16, ptr @__col_len, align 8
  %arr_header17 = call ptr @malloc(i64 24)
  %arr_data_raw18 = call ptr @malloc(i64 8)
  %len_ptr19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header17, i32 0, i32 0
  %cap_ptr20 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header17, i32 0, i32 1
  %data_field_ptr21 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header17, i32 0, i32 2
  store i64 0, ptr %len_ptr19, align 8
  store i64 0, ptr %cap_ptr20, align 8
  store ptr %arr_data_raw18, ptr %data_field_ptr21, align 8
  store ptr %arr_header17, ptr @__col_result, align 8
  store i64 0, ptr @__col_i, align 8
  br label %for.cond22

df_idx_ok:                                        ; preds = %for.body
  %elem_ptr = getelementptr ptr, ptr %data, i64 %__col_i_load2
  %record = load ptr, ptr %elem_ptr, align 8
  br label %df_idx_merge

df_idx_err:                                       ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg)
  br label %df_idx_merge

df_idx_merge:                                     ; preds = %df_idx_ok, %df_idx_err
  %df_idx_result = phi ptr [ null, %df_idx_err ], [ %record, %df_idx_ok ]
  store ptr %df_idx_result, ptr @__col_row, align 8
  %__col_result_load = load ptr, ptr @__col_result, align 8
  %__col_row_load = load ptr, ptr @__col_row, align 8
  %ptr_latitude = getelementptr ptr, ptr %__col_row_load, i64 1
  %val_latitude = load double, ptr %ptr_latitude, align 8
  %len_ptr4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load, i32 0, i32 0
  %cap_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load, i32 0, i32 1
  %data_ptr_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load, i32 0, i32 2
  %len7 = load i64, ptr %len_ptr4, align 8
  %cap = load i64, ptr %cap_ptr5, align 8
  %data8 = load ptr, ptr %data_ptr_ptr6, align 8
  %is_full = icmp uge i64 %len7, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %df_idx_merge
  %0 = icmp eq i64 %cap, 0
  %1 = mul i64 %cap, 2
  %new_cap = select i1 %0, i64 4, i64 %1
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data8, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr5, align 8
  store ptr %realloc, ptr %data_ptr_ptr6, align 8
  br label %cont

cont:                                             ; preds = %grow, %df_idx_merge
  %final_data_ptr = phi ptr [ %data8, %df_idx_merge ], [ %realloc, %grow ]
  %target_slot_ptr = getelementptr double, ptr %final_data_ptr, i64 %len7
  store double %val_latitude, ptr %target_slot_ptr, align 8
  %new_len = add i64 %len7, 1
  store i64 %new_len, ptr %len_ptr4, align 8
  br label %for.step

for.cond22:                                       ; preds = %for.step24, %for.end
  %__col_i_load26 = load i64, ptr @__col_i, align 8
  %__col_len_load27 = load i64, ptr @__col_len, align 8
  %icmp_tmp28 = icmp slt i64 %__col_i_load26, %__col_len_load27
  br i1 %icmp_tmp28, label %for.body23, label %for.end25

for.body23:                                       ; preds = %for.cond22
  %__col_src_load29 = load ptr, ptr @__col_src, align 8
  %__col_i_load30 = load i64, ptr @__col_i, align 8
  %rows_ptr_ptr31 = getelementptr inbounds nuw %dataframe, ptr %__col_src_load29, i32 0, i32 1
  %rows32 = load ptr, ptr %rows_ptr_ptr31, align 8
  %data_ptr_ptr33 = getelementptr inbounds nuw %array, ptr %rows32, i32 0, i32 2
  %data34 = load ptr, ptr %data_ptr_ptr33, align 8
  %len_ptr35 = getelementptr inbounds nuw %array, ptr %rows32, i32 0, i32 0
  %len36 = load i64, ptr %len_ptr35, align 8
  %in_bounds37 = icmp ult i64 %__col_i_load30, %len36
  br i1 %in_bounds37, label %df_idx_ok38, label %df_idx_err39

for.step24:                                       ; preds = %cont55
  %x_load62 = load i64, ptr @__col_i, align 8
  %inc_add63 = add i64 %x_load62, 1
  store i64 %inc_add63, ptr @__col_i, align 8
  br label %for.cond22, !llvm.loop !0

for.end25:                                        ; preds = %for.cond22
  %__col_result_load64 = load ptr, ptr @__col_result, align 8
  store ptr %__col_result_load64, ptr @__corr_y, align 8
  store i64 0, ptr @__corr_sum_x, align 8
  store i64 0, ptr @__corr_sum_y, align 8
  store i64 0, ptr @__corr_i, align 8
  store i64 0, ptr @__corr_i, align 8
  br label %for.cond65

df_idx_ok38:                                      ; preds = %for.body23
  %elem_ptr42 = getelementptr ptr, ptr %data34, i64 %__col_i_load30
  %record43 = load ptr, ptr %elem_ptr42, align 8
  br label %df_idx_merge40

df_idx_err39:                                     ; preds = %for.body23
  %print_err41 = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg.1)
  br label %df_idx_merge40

df_idx_merge40:                                   ; preds = %df_idx_ok38, %df_idx_err39
  %df_idx_result44 = phi ptr [ null, %df_idx_err39 ], [ %record43, %df_idx_ok38 ]
  store ptr %df_idx_result44, ptr @__col_row, align 8
  %__col_result_load45 = load ptr, ptr @__col_result, align 8
  %__col_row_load46 = load ptr, ptr @__col_row, align 8
  %ptr_wind-speed-max = getelementptr ptr, ptr %__col_row_load46, i64 4
  %val_wind-speed-max = load double, ptr %ptr_wind-speed-max, align 8
  %len_ptr47 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load45, i32 0, i32 0
  %cap_ptr48 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load45, i32 0, i32 1
  %data_ptr_ptr49 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__col_result_load45, i32 0, i32 2
  %len50 = load i64, ptr %len_ptr47, align 8
  %cap51 = load i64, ptr %cap_ptr48, align 8
  %data52 = load ptr, ptr %data_ptr_ptr49, align 8
  %is_full53 = icmp uge i64 %len50, %cap51
  br i1 %is_full53, label %grow54, label %cont55

grow54:                                           ; preds = %df_idx_merge40
  %2 = icmp eq i64 %cap51, 0
  %3 = mul i64 %cap51, 2
  %new_cap56 = select i1 %2, i64 4, i64 %3
  %bytes57 = mul i64 %new_cap56, 8
  %realloc58 = call ptr @realloc(ptr %data52, i64 %bytes57)
  store i64 %new_cap56, ptr %cap_ptr48, align 8
  store ptr %realloc58, ptr %data_ptr_ptr49, align 8
  br label %cont55

cont55:                                           ; preds = %grow54, %df_idx_merge40
  %final_data_ptr59 = phi ptr [ %data52, %df_idx_merge40 ], [ %realloc58, %grow54 ]
  %target_slot_ptr60 = getelementptr double, ptr %final_data_ptr59, i64 %len50
  store double %val_wind-speed-max, ptr %target_slot_ptr60, align 8
  %new_len61 = add i64 %len50, 1
  store i64 %new_len61, ptr %len_ptr47, align 8
  br label %for.step24

for.cond65:                                       ; preds = %for.step67, %for.end25
  %__corr_i_load = load i64, ptr @__corr_i, align 8
  %__corr_x_load = load ptr, ptr @__corr_x, align 8
  %len_ptr69 = getelementptr inbounds nuw %array, ptr %__corr_x_load, i32 0, i32 0
  %len70 = load i64, ptr %len_ptr69, align 8
  %icmp_tmp71 = icmp slt i64 %__corr_i_load, %len70
  br i1 %icmp_tmp71, label %for.body66, label %for.end68

for.body66:                                       ; preds = %for.cond65
  %__corr_sum_x_load = load double, ptr @__corr_sum_x, align 8
  %__corr_x_load72 = load ptr, ptr @__corr_x, align 8
  %__corr_i_load73 = load i64, ptr @__corr_i, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load72, i32 0, i32 0
  %data_field_ptr74 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load72, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_field_ptr74, align 8
  %array_len = load i64, ptr %len_field_ptr, align 8
  %4 = icmp slt i64 %__corr_i_load73, 0
  %5 = icmp sge i64 %__corr_i_load73, %array_len
  %is_invalid = or i1 %4, %5
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step67:                                       ; preds = %bounds.ok84
  %x_load89 = load i64, ptr @__corr_i, align 8
  %inc_add90 = add i64 %x_load89, 1
  store i64 %inc_add90, ptr @__corr_i, align 8
  br label %for.cond65, !llvm.loop !0

for.end68:                                        ; preds = %for.cond65
  %__corr_sum_x_load91 = load double, ptr @__corr_sum_x, align 8
  %__corr_x_load92 = load ptr, ptr @__corr_x, align 8
  %len_ptr93 = getelementptr inbounds nuw %array, ptr %__corr_x_load92, i32 0, i32 0
  %len94 = load i64, ptr %len_ptr93, align 8
  %int2double = sitofp i64 %len94 to double
  %fdivtmp = fdiv double %__corr_sum_x_load91, %int2double
  store double %fdivtmp, ptr @__corr_mean_x, align 8
  %__corr_sum_y_load95 = load double, ptr @__corr_sum_y, align 8
  %__corr_x_load96 = load ptr, ptr @__corr_x, align 8
  %len_ptr97 = getelementptr inbounds nuw %array, ptr %__corr_x_load96, i32 0, i32 0
  %len98 = load i64, ptr %len_ptr97, align 8
  %int2double99 = sitofp i64 %len98 to double
  %fdivtmp100 = fdiv double %__corr_sum_y_load95, %int2double99
  store double %fdivtmp100, ptr @__corr_mean_y, align 8
  store i64 0, ptr @__corr_i, align 8
  store i64 0, ptr @__corr_num, align 8
  store i64 0, ptr @__corr_dx, align 8
  store i64 0, ptr @__corr_dy, align 8
  store i64 0, ptr @__corr_i, align 8
  br label %for.cond101

bounds.fail:                                      ; preds = %for.body66
  %print_err75 = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body66
  %elem_ptr76 = getelementptr double, ptr %data_ptr, i64 %__corr_i_load73
  %loaded_val = load double, ptr %elem_ptr76, align 8
  %faddtmp = fadd double %__corr_sum_x_load, %loaded_val
  store double %faddtmp, ptr @__corr_sum_x, align 8
  %__corr_sum_y_load = load double, ptr @__corr_sum_y, align 8
  %__corr_y_load = load ptr, ptr @__corr_y, align 8
  %__corr_i_load77 = load i64, ptr @__corr_i, align 8
  %len_field_ptr78 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load, i32 0, i32 0
  %data_field_ptr79 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load, i32 0, i32 2
  %data_ptr80 = load ptr, ptr %data_field_ptr79, align 8
  %array_len81 = load i64, ptr %len_field_ptr78, align 8
  %6 = icmp slt i64 %__corr_i_load77, 0
  %7 = icmp sge i64 %__corr_i_load77, %array_len81
  %is_invalid82 = or i1 %6, %7
  br i1 %is_invalid82, label %bounds.fail83, label %bounds.ok84

bounds.fail83:                                    ; preds = %bounds.ok
  %print_err85 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok84:                                      ; preds = %bounds.ok
  %elem_ptr86 = getelementptr double, ptr %data_ptr80, i64 %__corr_i_load77
  %loaded_val87 = load double, ptr %elem_ptr86, align 8
  %faddtmp88 = fadd double %__corr_sum_y_load, %loaded_val87
  store double %faddtmp88, ptr @__corr_sum_y, align 8
  br label %for.step67

for.cond101:                                      ; preds = %for.step103, %for.end68
  %__corr_i_load105 = load i64, ptr @__corr_i, align 8
  %__corr_x_load106 = load ptr, ptr @__corr_x, align 8
  %len_ptr107 = getelementptr inbounds nuw %array, ptr %__corr_x_load106, i32 0, i32 0
  %len108 = load i64, ptr %len_ptr107, align 8
  %icmp_tmp109 = icmp slt i64 %__corr_i_load105, %len108
  br i1 %icmp_tmp109, label %for.body102, label %for.end104

for.body102:                                      ; preds = %for.cond101
  %__corr_num_load = load double, ptr @__corr_num, align 8
  %__corr_x_load110 = load ptr, ptr @__corr_x, align 8
  %__corr_i_load111 = load i64, ptr @__corr_i, align 8
  %len_field_ptr112 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load110, i32 0, i32 0
  %data_field_ptr113 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load110, i32 0, i32 2
  %data_ptr114 = load ptr, ptr %data_field_ptr113, align 8
  %array_len115 = load i64, ptr %len_field_ptr112, align 8
  %8 = icmp slt i64 %__corr_i_load111, 0
  %9 = icmp sge i64 %__corr_i_load111, %array_len115
  %is_invalid116 = or i1 %8, %9
  br i1 %is_invalid116, label %bounds.fail117, label %bounds.ok118

for.step103:                                      ; preds = %bounds.ok188
  %x_load196 = load i64, ptr @__corr_i, align 8
  %inc_add197 = add i64 %x_load196, 1
  store i64 %inc_add197, ptr @__corr_i, align 8
  br label %for.cond101, !llvm.loop !0

for.end104:                                       ; preds = %for.cond101
  %__corr_num_load198 = load double, ptr @__corr_num, align 8
  %__corr_dx_load199 = load double, ptr @__corr_dx, align 8
  %__corr_dy_load200 = load double, ptr @__corr_dy, align 8
  %fmultmp201 = fmul double %__corr_dx_load199, %__corr_dy_load200
  %sqrttmp = call double @llvm.sqrt.f64(double %fmultmp201)
  %fdivtmp202 = fdiv double %__corr_num_load198, %sqrttmp
  store double %fdivtmp202, ptr @__corr_result, align 8
  %__corr_result_load = load double, ptr @__corr_result, align 8
  %value_mem = call ptr @malloc(i64 8)
  store double %__corr_result_load, ptr %value_mem, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 2, ptr %tag_ptr, align 8
  %data_ptr203 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %value_mem, ptr %data_ptr203, align 8
  ret ptr %runtime_obj

bounds.fail117:                                   ; preds = %for.body102
  %print_err119 = call i32 (ptr, ...) @printf(ptr @err_msg.3)
  ret ptr null

bounds.ok118:                                     ; preds = %for.body102
  %elem_ptr120 = getelementptr double, ptr %data_ptr114, i64 %__corr_i_load111
  %loaded_val121 = load double, ptr %elem_ptr120, align 8
  %__corr_mean_x_load = load double, ptr @__corr_mean_x, align 8
  %fsubtmp = fsub double %loaded_val121, %__corr_mean_x_load
  %__corr_y_load122 = load ptr, ptr @__corr_y, align 8
  %__corr_i_load123 = load i64, ptr @__corr_i, align 8
  %len_field_ptr124 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load122, i32 0, i32 0
  %data_field_ptr125 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load122, i32 0, i32 2
  %data_ptr126 = load ptr, ptr %data_field_ptr125, align 8
  %array_len127 = load i64, ptr %len_field_ptr124, align 8
  %10 = icmp slt i64 %__corr_i_load123, 0
  %11 = icmp sge i64 %__corr_i_load123, %array_len127
  %is_invalid128 = or i1 %10, %11
  br i1 %is_invalid128, label %bounds.fail129, label %bounds.ok130

bounds.fail129:                                   ; preds = %bounds.ok118
  %print_err131 = call i32 (ptr, ...) @printf(ptr @err_msg.4)
  ret ptr null

bounds.ok130:                                     ; preds = %bounds.ok118
  %elem_ptr132 = getelementptr double, ptr %data_ptr126, i64 %__corr_i_load123
  %loaded_val133 = load double, ptr %elem_ptr132, align 8
  %__corr_mean_y_load = load double, ptr @__corr_mean_y, align 8
  %fsubtmp134 = fsub double %loaded_val133, %__corr_mean_y_load
  %fmultmp = fmul double %fsubtmp, %fsubtmp134
  %faddtmp135 = fadd double %__corr_num_load, %fmultmp
  store double %faddtmp135, ptr @__corr_num, align 8
  %__corr_dx_load = load double, ptr @__corr_dx, align 8
  %__corr_x_load136 = load ptr, ptr @__corr_x, align 8
  %__corr_i_load137 = load i64, ptr @__corr_i, align 8
  %len_field_ptr138 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load136, i32 0, i32 0
  %data_field_ptr139 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load136, i32 0, i32 2
  %data_ptr140 = load ptr, ptr %data_field_ptr139, align 8
  %array_len141 = load i64, ptr %len_field_ptr138, align 8
  %12 = icmp slt i64 %__corr_i_load137, 0
  %13 = icmp sge i64 %__corr_i_load137, %array_len141
  %is_invalid142 = or i1 %12, %13
  br i1 %is_invalid142, label %bounds.fail143, label %bounds.ok144

bounds.fail143:                                   ; preds = %bounds.ok130
  %print_err145 = call i32 (ptr, ...) @printf(ptr @err_msg.5)
  ret ptr null

bounds.ok144:                                     ; preds = %bounds.ok130
  %elem_ptr146 = getelementptr double, ptr %data_ptr140, i64 %__corr_i_load137
  %loaded_val147 = load double, ptr %elem_ptr146, align 8
  %__corr_mean_x_load148 = load double, ptr @__corr_mean_x, align 8
  %fsubtmp149 = fsub double %loaded_val147, %__corr_mean_x_load148
  %__corr_x_load150 = load ptr, ptr @__corr_x, align 8
  %__corr_i_load151 = load i64, ptr @__corr_i, align 8
  %len_field_ptr152 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load150, i32 0, i32 0
  %data_field_ptr153 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_x_load150, i32 0, i32 2
  %data_ptr154 = load ptr, ptr %data_field_ptr153, align 8
  %array_len155 = load i64, ptr %len_field_ptr152, align 8
  %14 = icmp slt i64 %__corr_i_load151, 0
  %15 = icmp sge i64 %__corr_i_load151, %array_len155
  %is_invalid156 = or i1 %14, %15
  br i1 %is_invalid156, label %bounds.fail157, label %bounds.ok158

bounds.fail157:                                   ; preds = %bounds.ok144
  %print_err159 = call i32 (ptr, ...) @printf(ptr @err_msg.6)
  ret ptr null

bounds.ok158:                                     ; preds = %bounds.ok144
  %elem_ptr160 = getelementptr double, ptr %data_ptr154, i64 %__corr_i_load151
  %loaded_val161 = load double, ptr %elem_ptr160, align 8
  %__corr_mean_x_load162 = load double, ptr @__corr_mean_x, align 8
  %fsubtmp163 = fsub double %loaded_val161, %__corr_mean_x_load162
  %fmultmp164 = fmul double %fsubtmp149, %fsubtmp163
  %faddtmp165 = fadd double %__corr_dx_load, %fmultmp164
  store double %faddtmp165, ptr @__corr_dx, align 8
  %__corr_dy_load = load double, ptr @__corr_dy, align 8
  %__corr_y_load166 = load ptr, ptr @__corr_y, align 8
  %__corr_i_load167 = load i64, ptr @__corr_i, align 8
  %len_field_ptr168 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load166, i32 0, i32 0
  %data_field_ptr169 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load166, i32 0, i32 2
  %data_ptr170 = load ptr, ptr %data_field_ptr169, align 8
  %array_len171 = load i64, ptr %len_field_ptr168, align 8
  %16 = icmp slt i64 %__corr_i_load167, 0
  %17 = icmp sge i64 %__corr_i_load167, %array_len171
  %is_invalid172 = or i1 %16, %17
  br i1 %is_invalid172, label %bounds.fail173, label %bounds.ok174

bounds.fail173:                                   ; preds = %bounds.ok158
  %print_err175 = call i32 (ptr, ...) @printf(ptr @err_msg.7)
  ret ptr null

bounds.ok174:                                     ; preds = %bounds.ok158
  %elem_ptr176 = getelementptr double, ptr %data_ptr170, i64 %__corr_i_load167
  %loaded_val177 = load double, ptr %elem_ptr176, align 8
  %__corr_mean_y_load178 = load double, ptr @__corr_mean_y, align 8
  %fsubtmp179 = fsub double %loaded_val177, %__corr_mean_y_load178
  %__corr_y_load180 = load ptr, ptr @__corr_y, align 8
  %__corr_i_load181 = load i64, ptr @__corr_i, align 8
  %len_field_ptr182 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load180, i32 0, i32 0
  %data_field_ptr183 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__corr_y_load180, i32 0, i32 2
  %data_ptr184 = load ptr, ptr %data_field_ptr183, align 8
  %array_len185 = load i64, ptr %len_field_ptr182, align 8
  %18 = icmp slt i64 %__corr_i_load181, 0
  %19 = icmp sge i64 %__corr_i_load181, %array_len185
  %is_invalid186 = or i1 %18, %19
  br i1 %is_invalid186, label %bounds.fail187, label %bounds.ok188

bounds.fail187:                                   ; preds = %bounds.ok174
  %print_err189 = call i32 (ptr, ...) @printf(ptr @err_msg.8)
  ret ptr null

bounds.ok188:                                     ; preds = %bounds.ok174
  %elem_ptr190 = getelementptr double, ptr %data_ptr184, i64 %__corr_i_load181
  %loaded_val191 = load double, ptr %elem_ptr190, align 8
  %__corr_mean_y_load192 = load double, ptr @__corr_mean_y, align 8
  %fsubtmp193 = fsub double %loaded_val191, %__corr_mean_y_load192
  %fmultmp194 = fmul double %fsubtmp179, %fsubtmp193
  %faddtmp195 = fadd double %__corr_dy_load, %fmultmp194
  store double %faddtmp195, ptr @__corr_dy, align 8
  br label %for.step103
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

; Function Attrs: nocallback nofree nosync nounwind speculatable willreturn memory(none)
declare double @llvm.sqrt.f64(double) #0

attributes #0 = { nocallback nofree nosync nounwind speculatable willreturn memory(none) }

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i1 true}
