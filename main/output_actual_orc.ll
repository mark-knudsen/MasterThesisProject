; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }
%struct_name_age_hasJob = type { ptr, i64, i8 }

@df2 = external global ptr
@__where_src = global ptr null, align 8
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.2 = private unnamed_addr constant [7 x i8] c"hasJob\00", align 1
@__where_result = global ptr null, align 8
@__where_i = global i64 0, align 8

define ptr @main_3() {
entry:
  %df2_load = load ptr, ptr @df2, align 8
  store ptr %df2_load, ptr @__where_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 32)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 3, ptr %len_ptr, align 8
  store i64 3, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data_raw, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data_raw, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr ptr, ptr %arr_data_raw, i64 2
  store ptr @str.2, ptr %elem_ptr2, align 8
  %arr_header3 = call ptr @malloc(i64 24)
  %arr_data_raw4 = call ptr @malloc(i64 800)
  %len_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 0
  %cap_ptr6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 1
  %data_field_ptr7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 2
  store i64 0, ptr %len_ptr5, align 8
  store i64 100, ptr %cap_ptr6, align 8
  store ptr %arr_data_raw4, ptr %data_field_ptr7, align 8
  %arr_header8 = call ptr @malloc(i64 24)
  %arr_data_raw9 = call ptr @malloc(i64 32)
  %len_ptr10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 0
  %cap_ptr11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 1
  %data_field_ptr12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header8, i32 0, i32 2
  store i64 3, ptr %len_ptr10, align 8
  store i64 3, ptr %cap_ptr11, align 8
  store ptr %arr_data_raw9, ptr %data_field_ptr12, align 8
  %elem_ptr13 = getelementptr i64, ptr %arr_data_raw9, i64 0
  store i64 4, ptr %elem_ptr13, align 8
  %elem_ptr14 = getelementptr i64, ptr %arr_data_raw9, i64 1
  store i64 1, ptr %elem_ptr14, align 8
  %elem_ptr15 = getelementptr i64, ptr %arr_data_raw9, i64 2
  store i64 3, ptr %elem_ptr15, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %cols_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  %rows_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  %types_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header, ptr %cols_gep, align 8
  store ptr %arr_header3, ptr %rows_gep, align 8
  store ptr %arr_header8, ptr %types_gep, align 8
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
  %fortest_int = icmp ne i1 %icmp_tmp, false
  br i1 %fortest_int, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__where_src_load16 = load ptr, ptr @__where_src, align 8
  %__where_i_load17 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__where_src_load16, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %elem_ptr18 = getelementptr ptr, ptr %data, i64 %__where_i_load17
  %record = load ptr, ptr %elem_ptr18, align 8
  %ptr_age = getelementptr %struct_name_age_hasJob, ptr %record, i32 0, i32 1
  %val_age = load i64, ptr %ptr_age, align 8
  %icmp_tmp19 = icmp sge i64 %val_age, 25
  br i1 %icmp_tmp19, label %then, label %else

for.step:                                         ; preds = %ifcont
  %x_load = load i64, ptr @__where_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__where_result_load32 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %__where_result_load32, ptr %data_ptr, align 8
  ret ptr %runtime_obj

then:                                             ; preds = %for.body
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load20 = load ptr, ptr @__where_src, align 8
  %__where_i_load21 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr22 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load20, i32 0, i32 1
  %rows23 = load ptr, ptr %rows_ptr_ptr22, align 8
  %data_ptr_ptr24 = getelementptr inbounds nuw %array, ptr %rows23, i32 0, i32 2
  %data25 = load ptr, ptr %data_ptr_ptr24, align 8
  %elem_ptr26 = getelementptr ptr, ptr %data25, i64 %__where_i_load21
  %record27 = load ptr, ptr %elem_ptr26, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr28 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr29 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr30 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len = load i64, ptr %len_ptr28, align 8
  %cap = load i64, ptr %cap_ptr29, align 8
  %data31 = load ptr, ptr %data_ptr_ptr30, align 8
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
  %realloc = call ptr @realloc(ptr %data31, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr29, align 8
  store ptr %realloc, ptr %data_ptr_ptr30, align 8
  br label %cont

cont:                                             ; preds = %grow, %then
  %data_phi = phi ptr [ %data31, %then ], [ %realloc, %grow ]
  %slot = getelementptr ptr, ptr %data_phi, i64 %len
  store ptr %record27, ptr %slot, align 8
  %new_len = add i64 %len, 1
  store i64 %new_len, ptr %len_ptr28, align 8
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i8 1}
