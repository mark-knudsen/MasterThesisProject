; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }
%struct_name_age = type { ptr, i64 }

@x = external global ptr
@__where_src = external global ptr, align 8
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@__where_result = external global ptr, align 8
@__where_i = external global i64, align 8
@str.2 = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.3 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.4 = private unnamed_addr constant [12 x i8] c"Hary potter\00", align 1

define ptr @main_6() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__where_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 32)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 2, ptr %len_ptr, align 8
  store i64 2, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data_raw, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data_raw, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %arr_header2 = call ptr @malloc(i64 24)
  %arr_data_raw3 = call ptr @malloc(i64 800)
  %len_ptr4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 0
  %cap_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 1
  %data_field_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header2, i32 0, i32 2
  store i64 0, ptr %len_ptr4, align 8
  store i64 100, ptr %cap_ptr5, align 8
  store ptr %arr_data_raw3, ptr %data_field_ptr6, align 8
  %arr_header7 = call ptr @malloc(i64 24)
  %arr_data_raw8 = call ptr @malloc(i64 32)
  %len_ptr9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 0
  %cap_ptr10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 1
  %data_field_ptr11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 2
  store i64 2, ptr %len_ptr9, align 8
  store i64 2, ptr %cap_ptr10, align 8
  store ptr %arr_data_raw8, ptr %data_field_ptr11, align 8
  %elem_ptr12 = getelementptr i64, ptr %arr_data_raw8, i64 0
  store i64 4, ptr %elem_ptr12, align 8
  %elem_ptr13 = getelementptr i64, ptr %arr_data_raw8, i64 1
  store i64 1, ptr %elem_ptr13, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %cols_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  %rows_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  %types_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header, ptr %cols_gep, align 8
  store ptr %arr_header2, ptr %rows_gep, align 8
  store ptr %arr_header7, ptr %types_gep, align 8
  store ptr %df, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__where_i_load = load i64, ptr @__where_i, align 8
  %__where_src_load = load ptr, ptr @__where_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__where_i_load, %rows_length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__where_src_load14 = load ptr, ptr @__where_src, align 8
  %__where_i_load15 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__where_src_load14, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %elem_ptr16 = getelementptr ptr, ptr %data, i64 %__where_i_load15
  %record = load ptr, ptr %elem_ptr16, align 8
  %ptr_age = getelementptr %struct_name_age, ptr %record, i32 0, i32 1
  %val_age = load i64, ptr %ptr_age, align 4
  %icmp_tmp17 = icmp sgt i64 %val_age, 91
  br i1 %icmp_tmp17, label %then, label %else

for.step:                                         ; preds = %ifcont
  %x_load30 = load i64, ptr @__where_i, align 8
  %inc_add = add i64 %x_load30, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__where_result_load31 = load ptr, ptr @__where_result, align 8
  store ptr %__where_result_load31, ptr @__where_src, align 8
  %arr_header32 = call ptr @malloc(i64 24)
  %arr_data_raw33 = call ptr @malloc(i64 32)
  %len_ptr34 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header32, i32 0, i32 0
  %cap_ptr35 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header32, i32 0, i32 1
  %data_field_ptr36 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header32, i32 0, i32 2
  store i64 2, ptr %len_ptr34, align 8
  store i64 2, ptr %cap_ptr35, align 8
  store ptr %arr_data_raw33, ptr %data_field_ptr36, align 8
  %elem_ptr37 = getelementptr ptr, ptr %arr_data_raw33, i64 0
  store ptr @str.2, ptr %elem_ptr37, align 8
  %elem_ptr38 = getelementptr ptr, ptr %arr_data_raw33, i64 1
  store ptr @str.3, ptr %elem_ptr38, align 8
  %arr_header39 = call ptr @malloc(i64 24)
  %arr_data_raw40 = call ptr @malloc(i64 800)
  %len_ptr41 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 0
  %cap_ptr42 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 1
  %data_field_ptr43 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 2
  store i64 0, ptr %len_ptr41, align 8
  store i64 100, ptr %cap_ptr42, align 8
  store ptr %arr_data_raw40, ptr %data_field_ptr43, align 8
  %arr_header44 = call ptr @malloc(i64 24)
  %arr_data_raw45 = call ptr @malloc(i64 32)
  %len_ptr46 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header44, i32 0, i32 0
  %cap_ptr47 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header44, i32 0, i32 1
  %data_field_ptr48 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header44, i32 0, i32 2
  store i64 2, ptr %len_ptr46, align 8
  store i64 2, ptr %cap_ptr47, align 8
  store ptr %arr_data_raw45, ptr %data_field_ptr48, align 8
  %elem_ptr49 = getelementptr i64, ptr %arr_data_raw45, i64 0
  store i64 4, ptr %elem_ptr49, align 8
  %elem_ptr50 = getelementptr i64, ptr %arr_data_raw45, i64 1
  store i64 1, ptr %elem_ptr50, align 8
  %df51 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %cols_gep52 = getelementptr inbounds nuw %dataframe, ptr %df51, i32 0, i32 0
  %rows_gep53 = getelementptr inbounds nuw %dataframe, ptr %df51, i32 0, i32 1
  %types_gep54 = getelementptr inbounds nuw %dataframe, ptr %df51, i32 0, i32 2
  store ptr %arr_header32, ptr %cols_gep52, align 8
  store ptr %arr_header39, ptr %rows_gep53, align 8
  store ptr %arr_header44, ptr %types_gep54, align 8
  store ptr %df51, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond55

then:                                             ; preds = %for.body
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load18 = load ptr, ptr @__where_src, align 8
  %__where_i_load19 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr20 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load18, i32 0, i32 1
  %rows21 = load ptr, ptr %rows_ptr_ptr20, align 8
  %data_ptr_ptr22 = getelementptr inbounds nuw %array, ptr %rows21, i32 0, i32 2
  %data23 = load ptr, ptr %data_ptr_ptr22, align 8
  %elem_ptr24 = getelementptr ptr, ptr %data23, i64 %__where_i_load19
  %record25 = load ptr, ptr %elem_ptr24, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr26 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr27 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr28 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len = load i64, ptr %len_ptr26, align 8
  %cap = load i64, ptr %cap_ptr27, align 8
  %data29 = load ptr, ptr %data_ptr_ptr28, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %cont

else:                                             ; preds = %for.body
  br label %ifcont

ifcont:                                           ; preds = %else, %cont
  %iftmp = phi ptr [ %__where_result_load, %cont ], [ 0.000000e+00, %else ]
  br label %for.step

grow:                                             ; preds = %then
  %0 = icmp eq i64 %cap, 0
  %1 = mul i64 %cap, 2
  %new_cap = select i1 %0, i64 4, i64 %1
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data29, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr27, align 8
  store ptr %realloc, ptr %data_ptr_ptr28, align 8
  br label %cont

cont:                                             ; preds = %grow, %then
  %final_data_ptr = phi ptr [ %data29, %then ], [ %realloc, %grow ]
  %target_slot_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %record25, ptr %target_slot_ptr, align 8
  %new_len = add i64 %len, 1
  store i64 %new_len, ptr %len_ptr26, align 8
  br label %ifcont

for.cond55:                                       ; preds = %for.step57, %for.end
  %__where_i_load59 = load i64, ptr @__where_i, align 8
  %__where_src_load60 = load ptr, ptr @__where_src, align 8
  %rows_ptr_field61 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_src_load60, i32 0, i32 1
  %rows_ptr62 = load ptr, ptr %rows_ptr_field61, align 8
  %rows_array_ptr63 = bitcast ptr %rows_ptr62 to ptr
  %rows_length_ptr64 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr63, i32 0, i32 0
  %rows_length65 = load i64, ptr %rows_length_ptr64, align 8
  %icmp_tmp66 = icmp slt i64 %__where_i_load59, %rows_length65
  br i1 %icmp_tmp66, label %for.body56, label %for.end58

for.body56:                                       ; preds = %for.cond55
  %__where_src_load67 = load ptr, ptr @__where_src, align 8
  %__where_i_load68 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr69 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load67, i32 0, i32 1
  %rows70 = load ptr, ptr %rows_ptr_ptr69, align 8
  %data_ptr_ptr71 = getelementptr inbounds nuw %array, ptr %rows70, i32 0, i32 2
  %data72 = load ptr, ptr %data_ptr_ptr71, align 8
  %elem_ptr73 = getelementptr ptr, ptr %data72, i64 %__where_i_load68
  %record74 = load ptr, ptr %elem_ptr73, align 8
  %ptr_age75 = getelementptr %struct_name_age, ptr %record74, i32 0, i32 1
  %val_age76 = load i64, ptr %ptr_age75, align 4
  %icmp_tmp77 = icmp slt i64 %val_age76, 93
  %__where_src_load78 = load ptr, ptr @__where_src, align 8
  %__where_i_load79 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr80 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load78, i32 0, i32 1
  %rows81 = load ptr, ptr %rows_ptr_ptr80, align 8
  %data_ptr_ptr82 = getelementptr inbounds nuw %array, ptr %rows81, i32 0, i32 2
  %data83 = load ptr, ptr %data_ptr_ptr82, align 8
  %elem_ptr84 = getelementptr ptr, ptr %data83, i64 %__where_i_load79
  %record85 = load ptr, ptr %elem_ptr84, align 8
  %ptr_name = getelementptr %struct_name_age, ptr %record85, i32 0, i32 0
  %val_name = load ptr, ptr %ptr_name, align 8
  %strcmp_res = call i32 @strcmp(ptr %val_name, ptr @str.4)
  %str_eq = icmp eq i32 %strcmp_res, 0
  %andtmp = and i1 %icmp_tmp77, %str_eq
  br i1 %andtmp, label %then86, label %else87

for.step57:                                       ; preds = %ifcont88
  %x_load116 = load i64, ptr @__where_i, align 8
  %inc_add117 = add i64 %x_load116, 1
  store i64 %inc_add117, ptr @__where_i, align 8
  br label %for.cond55, !llvm.loop !0

for.end58:                                        ; preds = %for.cond55
  %__where_result_load118 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %__where_result_load118, ptr %data_ptr, align 8
  ret ptr %runtime_obj

then86:                                           ; preds = %for.body56
  %__where_result_load89 = load ptr, ptr @__where_result, align 8
  %__where_src_load90 = load ptr, ptr @__where_src, align 8
  %__where_i_load91 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr92 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load90, i32 0, i32 1
  %rows93 = load ptr, ptr %rows_ptr_ptr92, align 8
  %data_ptr_ptr94 = getelementptr inbounds nuw %array, ptr %rows93, i32 0, i32 2
  %data95 = load ptr, ptr %data_ptr_ptr94, align 8
  %elem_ptr96 = getelementptr ptr, ptr %data95, i64 %__where_i_load91
  %record97 = load ptr, ptr %elem_ptr96, align 8
  %rows_field98 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_result_load89, i32 0, i32 1
  %rows_array99 = load ptr, ptr %rows_field98, align 8
  %len_ptr100 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array99, i32 0, i32 0
  %cap_ptr101 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array99, i32 0, i32 1
  %data_ptr_ptr102 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array99, i32 0, i32 2
  %len103 = load i64, ptr %len_ptr100, align 8
  %cap104 = load i64, ptr %cap_ptr101, align 8
  %data105 = load ptr, ptr %data_ptr_ptr102, align 8
  %is_full106 = icmp uge i64 %len103, %cap104
  br i1 %is_full106, label %grow107, label %cont108

else87:                                           ; preds = %for.body56
  br label %ifcont88

ifcont88:                                         ; preds = %else87, %cont108
  %iftmp115 = phi ptr [ %__where_result_load89, %cont108 ], [ 0.000000e+00, %else87 ]
  br label %for.step57

grow107:                                          ; preds = %then86
  %2 = icmp eq i64 %cap104, 0
  %3 = mul i64 %cap104, 2
  %new_cap109 = select i1 %2, i64 4, i64 %3
  %bytes110 = mul i64 %new_cap109, 8
  %realloc111 = call ptr @realloc(ptr %data105, i64 %bytes110)
  store i64 %new_cap109, ptr %cap_ptr101, align 8
  store ptr %realloc111, ptr %data_ptr_ptr102, align 8
  br label %cont108

cont108:                                          ; preds = %grow107, %then86
  %final_data_ptr112 = phi ptr [ %data105, %then86 ], [ %realloc111, %grow107 ]
  %target_slot_ptr113 = getelementptr ptr, ptr %final_data_ptr112, i64 %len103
  store ptr %record97, ptr %target_slot_ptr113, align 8
  %new_len114 = add i64 %len103, 1
  store i64 %new_len114, ptr %len_ptr100, align 8
  br label %ifcont88
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

declare i32 @strcmp(ptr, ptr)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i1 true}
