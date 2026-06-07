; ModuleID = 'repl_module'
source_filename = "repl_module"

%struct_name_age_hasJob_savings = type { ptr, i64, i8, double }
%dataframe = type { ptr, ptr, ptr, i64 }

@x = external global ptr
@__where_src = global ptr null
@__where_i = global i64 0
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@d = external global ptr
@__mask_count = global i64 0
@__mask = global i64 0
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.2 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.3 = private unnamed_addr constant [7 x i8] c"hasJob\00", align 1
@str.4 = private unnamed_addr constant [8 x i8] c"savings\00", align 1
@__where_result = global ptr null

define ptr @main_2() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__where_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %0, align 8
  %arr_data = call ptr @malloc(i64 100)
  store i64 100, ptr %1, align 8
  store ptr %arr_data, ptr %2, align 8
  %cols_field_ptr = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %arr_header, i32 0, i32 1
  %cols_array = load ptr, ptr %cols_field_ptr, align 8
  %cols_data_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %cols_array, i32 0, i32 2
  %cols_raw = load ptr, ptr %cols_data_ptr, align 8
  %columns = bitcast ptr %cols_raw to ptr
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %cols_array, i32 0, i32 0
  %column_count = load i64, ptr %len_ptr, align 4
  br label %loop

loop:                                             ; preds = %body, %entry
  %i = phi i64 [ 0, %entry ], [ %i_next, %body ]
  %cond = icmp slt i64 %i, %column_count
  br i1 %cond, label %body, label %exit

body:                                             ; preds = %loop
  %col_ptr_ptr = getelementptr ptr, ptr %columns, i64 %i
  %col = load ptr, ptr %col_ptr_ptr, align 8
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col, i32 0, i32 1
  store i64 0, ptr %cap_ptr, align 4
  %i_next = add i64 %i, 1
  br label %loop

exit:                                             ; preds = %loop
  store i64 0, ptr @__where_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %exit
  %__where_i_load = load i64, ptr @__where_i, align 8
  %__where_src_load = load ptr, ptr @__where_src, align 8
  %rowCount_ptr = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %__where_src_load, i32 0, i32 3
  %rowCount = load i64, ptr %rowCount_ptr, align 8
  %icmp_tmp = icmp slt i64 %__where_i_load, %rowCount
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %arr_header1 = call ptr @malloc(i64 24)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 0
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 1
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 2
  store i64 0, ptr %3, align 8
  %arr_data2 = call ptr @malloc(i64 100)
  store i64 100, ptr %4, align 8
  store ptr %arr_data2, ptr %5, align 8
  %__where_i_load3 = load i64, ptr @__where_i, align 8
  %len_ptr4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 0
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 2
  %array_len = load i64, ptr %len_ptr4, align 4
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %6 = icmp slt i64 %__where_i_load3, 0
  %7 = icmp sge i64 %__where_i_load3, %array_len
  %is_invalid = or i1 %6, %7
  br i1 %is_invalid, label %assign_bounds.fail, label %assign_bounds.ok

for.step:                                         ; preds = %assign_bounds.ok
  %x_load6 = load i64, ptr @__where_i, align 8
  %inc_add = add i64 %x_load6, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  store i64 0, ptr @__mask_count, align 8
  store i64 0, ptr @__mask, align 8
  br label %for.cond7

assign_bounds.fail:                               ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

assign_bounds.ok:                                 ; preds = %for.body
  %d_load = load ptr, ptr @d, align 8
  %ptr_age = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %d_load, i32 0, i32 1
  %val_age = load i64, ptr %ptr_age, align 4
  %icmp_tmp5 = icmp slt i64 %val_age, 30
  %bool_to_i8 = zext i1 %icmp_tmp5 to i8
  %elem_ptr = getelementptr i8, ptr %data_ptr, i64 %__where_i_load3
  store i8 %bool_to_i8, ptr %elem_ptr, align 1
  br label %for.step

for.cond7:                                        ; preds = %for.step9, %for.end
  %__where_i_load11 = load i64, ptr @__where_i, align 8
  %__where_src_load12 = load ptr, ptr @__where_src, align 8
  %rowCount_ptr13 = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %__where_src_load12, i32 0, i32 3
  %rowCount14 = load i64, ptr %rowCount_ptr13, align 8
  %icmp_tmp15 = icmp slt i64 %__where_i_load11, %rowCount14
  br i1 %icmp_tmp15, label %for.body8, label %for.end10

for.body8:                                        ; preds = %for.cond7
  %arr_header16 = call ptr @malloc(i64 24)
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header16, i32 0, i32 0
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header16, i32 0, i32 1
  %10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header16, i32 0, i32 2
  store i64 0, ptr %8, align 8
  %arr_data17 = call ptr @malloc(i64 100)
  store i64 100, ptr %9, align 8
  store ptr %arr_data17, ptr %10, align 8
  %__mask_load = load i64, ptr @__mask, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header16, i32 0, i32 0
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header16, i32 0, i32 2
  %data_ptr18 = load ptr, ptr %data_field_ptr, align 8
  %array_len19 = load i64, ptr %len_field_ptr, align 8
  %index_is_neg = icmp slt i64 %__mask_load, 0
  %index_rel = add i64 %__mask_load, %array_len19
  %resolved_index = select i1 %index_is_neg, i64 %index_rel, i64 %__mask_load
  %is_neg = icmp slt i64 %resolved_index, 0
  %is_too_big = icmp sge i64 %resolved_index, %array_len19
  %is_invalid20 = or i1 %is_neg, %is_too_big
  br i1 %is_invalid20, label %bounds.fail, label %bounds.ok

for.step9:                                        ; preds = %ifcont
  %x_load25 = load i64, ptr @__where_i, align 8
  %inc_add26 = add i64 %x_load25, 1
  store i64 %inc_add26, ptr @__where_i, align 8
  br label %for.cond7, !llvm.loop !2

for.end10:                                        ; preds = %for.cond7
  %arr_header27 = call ptr @malloc(i64 24)
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header27, i32 0, i32 0
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header27, i32 0, i32 1
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header27, i32 0, i32 2
  store i64 0, ptr %11, align 8
  %__mask_count_load = load i64, ptr @__mask_count, align 8
  %multmp = mul i64 %__mask_count_load, 8
  %arr_data28 = call ptr @malloc(i64 %multmp)
  store i64 %__mask_count_load, ptr %12, align 8
  store ptr %arr_data28, ptr %13, align 8
  %arr_header29 = call ptr @malloc(i64 24)
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header29, i32 0, i32 0
  %15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header29, i32 0, i32 1
  %16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header29, i32 0, i32 2
  store i64 0, ptr %14, align 8
  %__mask_count_load30 = load i64, ptr @__mask_count, align 8
  %multmp31 = mul i64 %__mask_count_load30, 8
  %arr_data32 = call ptr @malloc(i64 %multmp31)
  store i64 %__mask_count_load30, ptr %15, align 8
  store ptr %arr_data32, ptr %16, align 8
  %arr_header33 = call ptr @malloc(i64 24)
  %17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header33, i32 0, i32 0
  %18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header33, i32 0, i32 1
  %19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header33, i32 0, i32 2
  store i64 0, ptr %17, align 8
  %__mask_count_load34 = load i64, ptr @__mask_count, align 8
  %multmp35 = mul i64 %__mask_count_load34, 1
  %arr_data36 = call ptr @malloc(i64 %multmp35)
  store i64 %__mask_count_load34, ptr %18, align 8
  store ptr %arr_data36, ptr %19, align 8
  %arr_header37 = call ptr @malloc(i64 24)
  %20 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 0
  %21 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 1
  %22 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 2
  store i64 0, ptr %20, align 8
  %__mask_count_load38 = load i64, ptr @__mask_count, align 8
  %multmp39 = mul i64 %__mask_count_load38, 8
  %arr_data40 = call ptr @malloc(i64 %multmp39)
  store i64 %__mask_count_load38, ptr %21, align 8
  store ptr %arr_data40, ptr %22, align 8
  %data_header = call ptr @malloc(i64 32)
  %data_buffer = call ptr @malloc(i64 32)
  %23 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 0
  store i64 4, ptr %23, align 4
  %24 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 1
  store i64 4, ptr %24, align 4
  %25 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 2
  store ptr %data_buffer, ptr %25, align 8
  %data_gep = getelementptr ptr, ptr %data_buffer, i64 0
  store ptr %arr_header27, ptr %data_gep, align 8
  %data_gep41 = getelementptr ptr, ptr %data_buffer, i64 1
  store ptr %arr_header29, ptr %data_gep41, align 8
  %data_gep42 = getelementptr ptr, ptr %data_buffer, i64 2
  store ptr %arr_header33, ptr %data_gep42, align 8
  %data_gep43 = getelementptr ptr, ptr %data_buffer, i64 3
  store ptr %arr_header37, ptr %data_gep43, align 8
  %arr_header44 = call ptr @malloc(i64 24)
  %26 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header44, i32 0, i32 0
  %27 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header44, i32 0, i32 1
  %28 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header44, i32 0, i32 2
  store i64 4, ptr %26, align 8
  %arr_data45 = call ptr @malloc(i64 32)
  store i64 4, ptr %27, align 8
  store ptr %arr_data45, ptr %28, align 8
  %elem_ptr46 = getelementptr ptr, ptr %arr_data45, i64 0
  store ptr @str, ptr %elem_ptr46, align 8
  %elem_ptr47 = getelementptr ptr, ptr %arr_data45, i64 1
  store ptr @str.2, ptr %elem_ptr47, align 8
  %elem_ptr48 = getelementptr ptr, ptr %arr_data45, i64 2
  store ptr @str.3, ptr %elem_ptr48, align 8
  %elem_ptr49 = getelementptr ptr, ptr %arr_data45, i64 3
  store ptr @str.4, ptr %elem_ptr49, align 8
  %arr_header50 = call ptr @malloc(i64 24)
  %29 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header50, i32 0, i32 0
  %30 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header50, i32 0, i32 1
  %31 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header50, i32 0, i32 2
  store i64 4, ptr %29, align 8
  %arr_data51 = call ptr @malloc(i64 32)
  store i64 4, ptr %30, align 8
  store ptr %arr_data51, ptr %31, align 8
  %elem_ptr52 = getelementptr i64, ptr %arr_data51, i64 0
  store i64 4, ptr %elem_ptr52, align 8
  %elem_ptr53 = getelementptr i64, ptr %arr_data51, i64 1
  store i64 1, ptr %elem_ptr53, align 8
  %elem_ptr54 = getelementptr i64, ptr %arr_data51, i64 2
  store i64 3, ptr %elem_ptr54, align 8
  %elem_ptr55 = getelementptr i64, ptr %arr_data51, i64 3
  store i64 2, ptr %elem_ptr55, align 8
  %df_instance = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %32 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 0
  store ptr %arr_header44, ptr %32, align 8
  %33 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 1
  store ptr %data_header, ptr %33, align 8
  %34 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 2
  store ptr %arr_header50, ptr %34, align 8
  %35 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 3
  store i64 0, ptr %35, align 4
  store ptr %df_instance, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr56 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %df_instance, ptr %data_ptr56, align 8
  ret ptr %runtime_obj

bounds.fail:                                      ; preds = %for.body8
  %print_err21 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok:                                        ; preds = %for.body8
  %elem_ptr22 = getelementptr i8, ptr %data_ptr18, i64 %resolved_index
  %loaded_val = load i8, ptr %elem_ptr22, align 1
  br i8 %loaded_val, label %then, label %else

then:                                             ; preds = %bounds.ok
  %x_load23 = load i64, ptr @__mask_count, align 8
  %inc_add24 = add i64 %x_load23, 1
  store i64 %inc_add24, ptr @__mask_count, align 8
  br label %ifcont

else:                                             ; preds = %bounds.ok
  br label %ifcont

ifcont:                                           ; preds = %else, %then
  br label %for.step9
}

declare noalias ptr @malloc(i64)

!0 = distinct !{!0, !1}
!1 = !{!"llvm.loop.vectorize.enable", i1 true}
!2 = distinct !{!2, !1}
