; ModuleID = 'repl_module'
source_filename = "repl_module"

%array = type { i32, i32, ptr }

@df_lat = external global ptr
@__min_array = external global ptr, align 8
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@__min_val = external global double, align 8
@__min_i = external global i64, align 8
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@df_min = external global double, align 8
@__max_array = external global ptr, align 8
@err_msg.3 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@__max_val = external global double, align 8
@__max_i = external global i64, align 8
@err_msg.4 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.5 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@df_max = external global double, align 8
@__mean_array = external global ptr, align 8
@__mean_sum = external global double, align 8
@__mean_i = external global i64, align 8
@err_msg.6 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@__mean_val = external global double, align 8
@df_mean = external global double, align 8
@__sum_array = external global ptr, align 8
@__sum_val = external global i64, align 8
@__sum_i = external global i64, align 8
@err_msg.7 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@df_sum = external global double, align 8

define ptr @main_8() {
entry:
  %df_lat_load = load ptr, ptr @df_lat, align 8
  store ptr %df_lat_load, ptr @__min_array, align 8
  %__min_array_load = load ptr, ptr @__min_array, align 8
  %len_field_ptr = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__min_array_load, i32 0, i32 0
  %data_field_ptr = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__min_array_load, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_field_ptr, align 8
  %array_len = load i64, ptr %len_field_ptr, align 8
  %index_rel = add i64 0, %array_len
  %resolved_index = select i1 false, i64 %index_rel, i64 0
  %is_neg = icmp slt i64 %resolved_index, 0
  %is_too_big = icmp sge i64 %resolved_index, %array_len
  %is_invalid = or i1 %is_neg, %is_too_big
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

bounds.fail:                                      ; preds = %entry
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %entry
  %elem_ptr = getelementptr double, ptr %data_ptr, i64 %resolved_index
  %loaded_val = load double, ptr %elem_ptr, align 8
  store double %loaded_val, ptr @__min_val, align 8
  store i64 1, ptr @__min_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %bounds.ok
  %__min_i_load = load i64, ptr @__min_i, align 8
  %__min_array_load1 = load ptr, ptr @__min_array, align 8
  %len_ptr = getelementptr inbounds nuw %array, ptr %__min_array_load1, i32 0, i32 0
  %len32 = load i32, ptr %len_ptr, align 4
  %len = zext i32 %len32 to i64
  %icmp_tmp = icmp slt i64 %__min_i_load, %len
  %fortest_int = icmp ne i1 %icmp_tmp, false
  br i1 %fortest_int, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__min_array_load2 = load ptr, ptr @__min_array, align 8
  %__min_i_load3 = load i64, ptr @__min_i, align 8
  %len_field_ptr4 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__min_array_load2, i32 0, i32 0
  %data_field_ptr5 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__min_array_load2, i32 0, i32 2
  %data_ptr6 = load ptr, ptr %data_field_ptr5, align 8
  %array_len7 = load i64, ptr %len_field_ptr4, align 8
  %index_is_neg = icmp slt i64 %__min_i_load3, 0
  %index_rel8 = add i64 %__min_i_load3, %array_len7
  %resolved_index9 = select i1 %index_is_neg, i64 %index_rel8, i64 %__min_i_load3
  %is_neg10 = icmp slt i64 %resolved_index9, 0
  %is_too_big11 = icmp sge i64 %resolved_index9, %array_len7
  %is_invalid12 = or i1 %is_neg10, %is_too_big11
  br i1 %is_invalid12, label %bounds.fail13, label %bounds.ok14

for.step:                                         ; preds = %ifcont
  %x_load = load i64, ptr @__min_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__min_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__min_val_load35 = load double, ptr @__min_val, align 8
  store double %__min_val_load35, ptr @df_min, align 8
  %df_lat_load36 = load ptr, ptr @df_lat, align 8
  store ptr %df_lat_load36, ptr @__max_array, align 8
  %__max_array_load = load ptr, ptr @__max_array, align 8
  %len_field_ptr37 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__max_array_load, i32 0, i32 0
  %data_field_ptr38 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__max_array_load, i32 0, i32 2
  %data_ptr39 = load ptr, ptr %data_field_ptr38, align 8
  %array_len40 = load i64, ptr %len_field_ptr37, align 8
  %index_rel41 = add i64 0, %array_len40
  %resolved_index42 = select i1 false, i64 %index_rel41, i64 0
  %is_neg43 = icmp slt i64 %resolved_index42, 0
  %is_too_big44 = icmp sge i64 %resolved_index42, %array_len40
  %is_invalid45 = or i1 %is_neg43, %is_too_big44
  br i1 %is_invalid45, label %bounds.fail46, label %bounds.ok47

bounds.fail13:                                    ; preds = %for.body
  %print_err15 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok14:                                      ; preds = %for.body
  %elem_ptr16 = getelementptr double, ptr %data_ptr6, i64 %resolved_index9
  %loaded_val17 = load double, ptr %elem_ptr16, align 8
  %__min_val_load = load double, ptr @__min_val, align 8
  %fcmp_tmp = fcmp olt double %loaded_val17, %__min_val_load
  br i1 %fcmp_tmp, label %then, label %else

then:                                             ; preds = %bounds.ok14
  %__min_array_load18 = load ptr, ptr @__min_array, align 8
  %__min_i_load19 = load i64, ptr @__min_i, align 8
  %len_field_ptr20 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__min_array_load18, i32 0, i32 0
  %data_field_ptr21 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__min_array_load18, i32 0, i32 2
  %data_ptr22 = load ptr, ptr %data_field_ptr21, align 8
  %array_len23 = load i64, ptr %len_field_ptr20, align 8
  %index_is_neg24 = icmp slt i64 %__min_i_load19, 0
  %index_rel25 = add i64 %__min_i_load19, %array_len23
  %resolved_index26 = select i1 %index_is_neg24, i64 %index_rel25, i64 %__min_i_load19
  %is_neg27 = icmp slt i64 %resolved_index26, 0
  %is_too_big28 = icmp sge i64 %resolved_index26, %array_len23
  %is_invalid29 = or i1 %is_neg27, %is_too_big28
  br i1 %is_invalid29, label %bounds.fail30, label %bounds.ok31

else:                                             ; preds = %bounds.ok14
  br label %ifcont

ifcont:                                           ; preds = %else, %bounds.ok31
  %iftmp = phi double [ %loaded_val34, %bounds.ok31 ], [ 0.000000e+00, %else ]
  br label %for.step

bounds.fail30:                                    ; preds = %then
  %print_err32 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok31:                                      ; preds = %then
  %elem_ptr33 = getelementptr double, ptr %data_ptr22, i64 %resolved_index26
  %loaded_val34 = load double, ptr %elem_ptr33, align 8
  store double %loaded_val34, ptr @__min_val, align 8
  br label %ifcont

bounds.fail46:                                    ; preds = %for.end
  %print_err48 = call i32 (ptr, ...) @printf(ptr @err_msg.3)
  ret ptr null

bounds.ok47:                                      ; preds = %for.end
  %elem_ptr49 = getelementptr double, ptr %data_ptr39, i64 %resolved_index42
  %loaded_val50 = load double, ptr %elem_ptr49, align 8
  store double %loaded_val50, ptr @__max_val, align 8
  store i64 1, ptr @__max_i, align 8
  br label %for.cond51

for.cond51:                                       ; preds = %for.step53, %bounds.ok47
  %__max_i_load = load i64, ptr @__max_i, align 8
  %__max_array_load55 = load ptr, ptr @__max_array, align 8
  %len_ptr56 = getelementptr inbounds nuw %array, ptr %__max_array_load55, i32 0, i32 0
  %len3257 = load i32, ptr %len_ptr56, align 4
  %len58 = zext i32 %len3257 to i64
  %icmp_tmp59 = icmp slt i64 %__max_i_load, %len58
  %fortest_int60 = icmp ne i1 %icmp_tmp59, false
  br i1 %fortest_int60, label %for.body52, label %for.end54

for.body52:                                       ; preds = %for.cond51
  %__max_array_load61 = load ptr, ptr @__max_array, align 8
  %__max_i_load62 = load i64, ptr @__max_i, align 8
  %len_field_ptr63 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__max_array_load61, i32 0, i32 0
  %data_field_ptr64 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__max_array_load61, i32 0, i32 2
  %data_ptr65 = load ptr, ptr %data_field_ptr64, align 8
  %array_len66 = load i64, ptr %len_field_ptr63, align 8
  %index_is_neg67 = icmp slt i64 %__max_i_load62, 0
  %index_rel68 = add i64 %__max_i_load62, %array_len66
  %resolved_index69 = select i1 %index_is_neg67, i64 %index_rel68, i64 %__max_i_load62
  %is_neg70 = icmp slt i64 %resolved_index69, 0
  %is_too_big71 = icmp sge i64 %resolved_index69, %array_len66
  %is_invalid72 = or i1 %is_neg70, %is_too_big71
  br i1 %is_invalid72, label %bounds.fail73, label %bounds.ok74

for.step53:                                       ; preds = %ifcont81
  %x_load100 = load i64, ptr @__max_i, align 8
  %inc_add101 = add i64 %x_load100, 1
  store i64 %inc_add101, ptr @__max_i, align 8
  br label %for.cond51, !llvm.loop !0

for.end54:                                        ; preds = %for.cond51
  %__max_val_load102 = load double, ptr @__max_val, align 8
  store double %__max_val_load102, ptr @df_max, align 8
  %df_lat_load103 = load ptr, ptr @df_lat, align 8
  store ptr %df_lat_load103, ptr @__mean_array, align 8
  store double 0.000000e+00, ptr @__mean_sum, align 8
  store i64 0, ptr @__mean_i, align 8
  br label %for.cond104

bounds.fail73:                                    ; preds = %for.body52
  %print_err75 = call i32 (ptr, ...) @printf(ptr @err_msg.4)
  ret ptr null

bounds.ok74:                                      ; preds = %for.body52
  %elem_ptr76 = getelementptr double, ptr %data_ptr65, i64 %resolved_index69
  %loaded_val77 = load double, ptr %elem_ptr76, align 8
  %__max_val_load = load double, ptr @__max_val, align 8
  %fcmp_tmp78 = fcmp ogt double %loaded_val77, %__max_val_load
  br i1 %fcmp_tmp78, label %then79, label %else80

then79:                                           ; preds = %bounds.ok74
  %__max_array_load82 = load ptr, ptr @__max_array, align 8
  %__max_i_load83 = load i64, ptr @__max_i, align 8
  %len_field_ptr84 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__max_array_load82, i32 0, i32 0
  %data_field_ptr85 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__max_array_load82, i32 0, i32 2
  %data_ptr86 = load ptr, ptr %data_field_ptr85, align 8
  %array_len87 = load i64, ptr %len_field_ptr84, align 8
  %index_is_neg88 = icmp slt i64 %__max_i_load83, 0
  %index_rel89 = add i64 %__max_i_load83, %array_len87
  %resolved_index90 = select i1 %index_is_neg88, i64 %index_rel89, i64 %__max_i_load83
  %is_neg91 = icmp slt i64 %resolved_index90, 0
  %is_too_big92 = icmp sge i64 %resolved_index90, %array_len87
  %is_invalid93 = or i1 %is_neg91, %is_too_big92
  br i1 %is_invalid93, label %bounds.fail94, label %bounds.ok95

else80:                                           ; preds = %bounds.ok74
  br label %ifcont81

ifcont81:                                         ; preds = %else80, %bounds.ok95
  %iftmp99 = phi double [ %loaded_val98, %bounds.ok95 ], [ 0.000000e+00, %else80 ]
  br label %for.step53

bounds.fail94:                                    ; preds = %then79
  %print_err96 = call i32 (ptr, ...) @printf(ptr @err_msg.5)
  ret ptr null

bounds.ok95:                                      ; preds = %then79
  %elem_ptr97 = getelementptr double, ptr %data_ptr86, i64 %resolved_index90
  %loaded_val98 = load double, ptr %elem_ptr97, align 8
  store double %loaded_val98, ptr @__max_val, align 8
  br label %ifcont81

for.cond104:                                      ; preds = %for.step106, %for.end54
  %__mean_i_load = load i64, ptr @__mean_i, align 8
  %__mean_array_load = load ptr, ptr @__mean_array, align 8
  %len_ptr108 = getelementptr inbounds nuw %array, ptr %__mean_array_load, i32 0, i32 0
  %len32109 = load i32, ptr %len_ptr108, align 4
  %len110 = zext i32 %len32109 to i64
  %icmp_tmp111 = icmp slt i64 %__mean_i_load, %len110
  %fortest_int112 = icmp ne i1 %icmp_tmp111, false
  br i1 %fortest_int112, label %for.body105, label %for.end107

for.body105:                                      ; preds = %for.cond104
  %__mean_sum_load = load double, ptr @__mean_sum, align 8
  %__mean_array_load113 = load ptr, ptr @__mean_array, align 8
  %__mean_i_load114 = load i64, ptr @__mean_i, align 8
  %len_field_ptr115 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__mean_array_load113, i32 0, i32 0
  %data_field_ptr116 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__mean_array_load113, i32 0, i32 2
  %data_ptr117 = load ptr, ptr %data_field_ptr116, align 8
  %array_len118 = load i64, ptr %len_field_ptr115, align 8
  %index_is_neg119 = icmp slt i64 %__mean_i_load114, 0
  %index_rel120 = add i64 %__mean_i_load114, %array_len118
  %resolved_index121 = select i1 %index_is_neg119, i64 %index_rel120, i64 %__mean_i_load114
  %is_neg122 = icmp slt i64 %resolved_index121, 0
  %is_too_big123 = icmp sge i64 %resolved_index121, %array_len118
  %is_invalid124 = or i1 %is_neg122, %is_too_big123
  br i1 %is_invalid124, label %bounds.fail125, label %bounds.ok126

for.step106:                                      ; preds = %bounds.ok126
  %x_load130 = load i64, ptr @__mean_i, align 8
  %inc_add131 = add i64 %x_load130, 1
  store i64 %inc_add131, ptr @__mean_i, align 8
  br label %for.cond104, !llvm.loop !0

for.end107:                                       ; preds = %for.cond104
  %__mean_sum_load132 = load double, ptr @__mean_sum, align 8
  %__mean_array_load133 = load ptr, ptr @__mean_array, align 8
  %len_ptr134 = getelementptr inbounds nuw %array, ptr %__mean_array_load133, i32 0, i32 0
  %len32135 = load i32, ptr %len_ptr134, align 4
  %len136 = zext i32 %len32135 to i64
  %int2double = sitofp i64 %len136 to double
  %fdivtmp = fdiv double %__mean_sum_load132, %int2double
  store double %fdivtmp, ptr @__mean_val, align 8
  %__mean_val_load = load double, ptr @__mean_val, align 8
  store double %__mean_val_load, ptr @df_mean, align 8
  %df_lat_load137 = load ptr, ptr @df_lat, align 8
  store ptr %df_lat_load137, ptr @__sum_array, align 8
  store i64 0, ptr @__sum_val, align 8
  store i64 0, ptr @__sum_i, align 8
  br label %for.cond138

bounds.fail125:                                   ; preds = %for.body105
  %print_err127 = call i32 (ptr, ...) @printf(ptr @err_msg.6)
  ret ptr null

bounds.ok126:                                     ; preds = %for.body105
  %elem_ptr128 = getelementptr double, ptr %data_ptr117, i64 %resolved_index121
  %loaded_val129 = load double, ptr %elem_ptr128, align 8
  %faddtmp = fadd double %__mean_sum_load, %loaded_val129
  store double %faddtmp, ptr @__mean_sum, align 8
  br label %for.step106

for.cond138:                                      ; preds = %for.step140, %for.end107
  %__sum_i_load = load i64, ptr @__sum_i, align 8
  %__sum_array_load = load ptr, ptr @__sum_array, align 8
  %len_ptr142 = getelementptr inbounds nuw %array, ptr %__sum_array_load, i32 0, i32 0
  %len32143 = load i32, ptr %len_ptr142, align 4
  %len144 = zext i32 %len32143 to i64
  %icmp_tmp145 = icmp slt i64 %__sum_i_load, %len144
  %fortest_int146 = icmp ne i1 %icmp_tmp145, false
  br i1 %fortest_int146, label %for.body139, label %for.end141

for.body139:                                      ; preds = %for.cond138
  %__sum_val_load = load double, ptr @__sum_val, align 8
  %__sum_array_load147 = load ptr, ptr @__sum_array, align 8
  %__sum_i_load148 = load i64, ptr @__sum_i, align 8
  %len_field_ptr149 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__sum_array_load147, i32 0, i32 0
  %data_field_ptr150 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__sum_array_load147, i32 0, i32 2
  %data_ptr151 = load ptr, ptr %data_field_ptr150, align 8
  %array_len152 = load i64, ptr %len_field_ptr149, align 8
  %index_is_neg153 = icmp slt i64 %__sum_i_load148, 0
  %index_rel154 = add i64 %__sum_i_load148, %array_len152
  %resolved_index155 = select i1 %index_is_neg153, i64 %index_rel154, i64 %__sum_i_load148
  %is_neg156 = icmp slt i64 %resolved_index155, 0
  %is_too_big157 = icmp sge i64 %resolved_index155, %array_len152
  %is_invalid158 = or i1 %is_neg156, %is_too_big157
  br i1 %is_invalid158, label %bounds.fail159, label %bounds.ok160

for.step140:                                      ; preds = %bounds.ok160
  %x_load165 = load i64, ptr @__sum_i, align 8
  %inc_add166 = add i64 %x_load165, 1
  store i64 %inc_add166, ptr @__sum_i, align 8
  br label %for.cond138, !llvm.loop !0

for.end141:                                       ; preds = %for.cond138
  %__sum_val_load167 = load double, ptr @__sum_val, align 8
  store double %__sum_val_load167, ptr @df_sum, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr168 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr168, align 8
  ret ptr %runtime_obj

bounds.fail159:                                   ; preds = %for.body139
  %print_err161 = call i32 (ptr, ...) @printf(ptr @err_msg.7)
  ret ptr null

bounds.ok160:                                     ; preds = %for.body139
  %elem_ptr162 = getelementptr double, ptr %data_ptr151, i64 %resolved_index155
  %loaded_val163 = load double, ptr %elem_ptr162, align 8
  %faddtmp164 = fadd double %__sum_val_load, %loaded_val163
  store double %faddtmp164, ptr @__sum_val, align 8
  br label %for.step140
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i32)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i8 1}
