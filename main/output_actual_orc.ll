; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }
%struct_savings_future_savings = type { i64, double }
%struct_name_age_hasJob_savings = type { ptr, i64, i8, double }

@x = external global ptr
@__map_src = global ptr null, align 8
@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.2 = private unnamed_addr constant [7 x i8] c"hasJob\00", align 1
@str.3 = private unnamed_addr constant [8 x i8] c"savings\00", align 1
@str.4 = private unnamed_addr constant [15 x i8] c"future_savings\00", align 1
@__map_result = global ptr null, align 8
@__map_i = global i64 0, align 8
@__current_row = global ptr null, align 8

define ptr @main_2() {
entry:
  %x_load = load ptr, ptr @x, align 8
  store ptr %x_load, ptr @__map_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 96)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 5, ptr %len_ptr, align 8
  store i64 10, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data_raw, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr1 = getelementptr ptr, ptr %arr_data_raw, i64 1
  store ptr @str.1, ptr %elem_ptr1, align 8
  %elem_ptr2 = getelementptr ptr, ptr %arr_data_raw, i64 2
  store ptr @str.2, ptr %elem_ptr2, align 8
  %elem_ptr3 = getelementptr ptr, ptr %arr_data_raw, i64 3
  store ptr @str.3, ptr %elem_ptr3, align 8
  %elem_ptr4 = getelementptr ptr, ptr %arr_data_raw, i64 4
  store ptr @str.4, ptr %elem_ptr4, align 8
  %arr_header5 = call ptr @malloc(i64 24)
  %arr_data_raw6 = call ptr @malloc(i64 800)
  %len_ptr7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 0
  %cap_ptr8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 1
  %data_field_ptr9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 2
  store i64 0, ptr %len_ptr7, align 8
  store i64 100, ptr %cap_ptr8, align 8
  store ptr %arr_data_raw6, ptr %data_field_ptr9, align 8
  %arr_header10 = call ptr @malloc(i64 24)
  %arr_data_raw11 = call ptr @malloc(i64 96)
  %len_ptr12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header10, i32 0, i32 0
  %cap_ptr13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header10, i32 0, i32 1
  %data_field_ptr14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header10, i32 0, i32 2
  store i64 5, ptr %len_ptr12, align 8
  store i64 10, ptr %cap_ptr13, align 8
  store ptr %arr_data_raw11, ptr %data_field_ptr14, align 8
  %elem_ptr15 = getelementptr i64, ptr %arr_data_raw11, i64 0
  store i64 4, ptr %elem_ptr15, align 8
  %elem_ptr16 = getelementptr i64, ptr %arr_data_raw11, i64 1
  store i64 1, ptr %elem_ptr16, align 8
  %elem_ptr17 = getelementptr i64, ptr %arr_data_raw11, i64 2
  store i64 3, ptr %elem_ptr17, align 8
  %elem_ptr18 = getelementptr i64, ptr %arr_data_raw11, i64 3
  store i64 1, ptr %elem_ptr18, align 8
  %elem_ptr19 = getelementptr i64, ptr %arr_data_raw11, i64 4
  store i64 2, ptr %elem_ptr19, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %cols_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  %rows_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  %types_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header, ptr %cols_gep, align 8
  store ptr %arr_header5, ptr %rows_gep, align 8
  store ptr %arr_header10, ptr %types_gep, align 8
  store ptr %df, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__map_i_load = load i64, ptr @__map_i, align 8
  %__map_src_load = load ptr, ptr @__map_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %len_ptr20 = getelementptr inbounds nuw %array, ptr %rows_ptr, i32 0, i32 0
  %len = load i64, ptr %len_ptr20, align 8
  %icmp_tmp = icmp slt i64 %__map_i_load, %len
  %fortest_int = icmp ne i1 %icmp_tmp, false
  br i1 %fortest_int, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_src_load21 = load ptr, ptr @__map_src, align 8
  %__map_i_load22 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load21, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %elem_ptr23 = getelementptr ptr, ptr %data, i64 %__map_i_load22
  %record = load ptr, ptr %elem_ptr23, align 8
  store ptr %record, ptr @__current_row, align 8
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %__current_row_load = load ptr, ptr @__current_row, align 8
  %record_ptr = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_savings_future_savings, ptr null, i32 1) to i64))
  %field_0 = getelementptr %struct_savings_future_savings, ptr %record_ptr, i32 0, i32 0
  store i64 0, ptr %field_0, align 8
  %__current_row_load24 = load ptr, ptr @__current_row, align 8
  %ptr_savings = getelementptr %struct_name_age_hasJob_savings, ptr %__current_row_load24, i32 0, i32 3
  %val_savings = load double, ptr %ptr_savings, align 8
  %fmultmp = fmul double %val_savings, 1.500000e+00
  %field_1 = getelementptr %struct_savings_future_savings, ptr %record_ptr, i32 0, i32 1
  store double %fmultmp, ptr %field_1, align 8
  %__current_row_load25 = load ptr, ptr @__current_row, align 8
  %record_ptr26 = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_savings_future_savings, ptr null, i32 1) to i64))
  %field_027 = getelementptr %struct_savings_future_savings, ptr %record_ptr26, i32 0, i32 0
  store i64 0, ptr %field_027, align 8
  %__current_row_load28 = load ptr, ptr @__current_row, align 8
  %ptr_savings29 = getelementptr %struct_name_age_hasJob_savings, ptr %__current_row_load28, i32 0, i32 3
  %val_savings30 = load double, ptr %ptr_savings29, align 8
  %fmultmp31 = fmul double %val_savings30, 1.500000e+00
  %field_132 = getelementptr %struct_savings_future_savings, ptr %record_ptr26, i32 0, i32 1
  store double %fmultmp31, ptr %field_132, align 8
  %0 = getelementptr ptr, ptr %__current_row_load25, i64 0
  %loaded_field = load ptr, ptr %0, align 8
  %1 = getelementptr ptr, ptr %__current_row_load25, i64 1
  %loaded_field33 = load ptr, ptr %1, align 8
  %2 = getelementptr ptr, ptr %__current_row_load25, i64 2
  %loaded_field34 = load ptr, ptr %2, align 8
  %3 = getelementptr ptr, ptr %record_ptr26, i64 0
  %loaded_field35 = load ptr, ptr %3, align 8
  %4 = getelementptr ptr, ptr %record_ptr26, i64 1
  %loaded_field36 = load ptr, ptr %4, align 8
  %rec_ptr = call ptr @malloc(i64 40)
  %slot_0 = getelementptr ptr, ptr %rec_ptr, i64 0
  store ptr %loaded_field, ptr %slot_0, align 8
  %slot_1 = getelementptr ptr, ptr %rec_ptr, i64 1
  store ptr %loaded_field33, ptr %slot_1, align 8
  %slot_2 = getelementptr ptr, ptr %rec_ptr, i64 2
  store ptr %loaded_field34, ptr %slot_2, align 8
  %slot_3 = getelementptr ptr, ptr %rec_ptr, i64 3
  store ptr %loaded_field35, ptr %slot_3, align 8
  %slot_4 = getelementptr ptr, ptr %rec_ptr, i64 4
  store ptr %loaded_field36, ptr %slot_4, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr37 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr38 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr39 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len40 = load i64, ptr %len_ptr37, align 8
  %cap = load i64, ptr %cap_ptr38, align 8
  %data41 = load ptr, ptr %data_ptr_ptr39, align 8
  %is_full = icmp uge i64 %len40, %cap
  br i1 %is_full, label %grow, label %cont

for.step:                                         ; preds = %cont
  %x_load42 = load i64, ptr @__map_i, align 8
  %inc_add = add i64 %x_load42, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__map_result_load43 = load ptr, ptr @__map_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %__map_result_load43, ptr %data_ptr, align 8
  ret ptr %runtime_obj

grow:                                             ; preds = %for.body
  %5 = icmp eq i64 %cap, 0
  %6 = mul i64 %cap, 2
  %new_cap = select i1 %5, i64 4, i64 %6
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data41, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr38, align 8
  store ptr %realloc, ptr %data_ptr_ptr39, align 8
  br label %cont

cont:                                             ; preds = %grow, %for.body
  %data_phi = phi ptr [ %data41, %for.body ], [ %realloc, %grow ]
  %slot = getelementptr ptr, ptr %data_phi, i64 %len40
  store ptr %rec_ptr, ptr %slot, align 8
  %new_len = add i64 %len40, 1
  store i64 %new_len, ptr %len_ptr37, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i8 1}
