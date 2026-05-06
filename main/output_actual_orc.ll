; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@df2 = external global ptr
@__map_src = external global ptr, align 8
@str = private unnamed_addr constant [5 x i8] c"date\00", align 1
@str.1 = private unnamed_addr constant [9 x i8] c"latitude\00", align 1
@str.2 = private unnamed_addr constant [10 x i8] c"longitude\00", align 1
@str.3 = private unnamed_addr constant [11 x i8] c"fire_label\00", align 1
@str.4 = private unnamed_addr constant [20 x i8] c"land_cover_class_17\00", align 1
@__map_result = external global ptr, align 8
@__map_i = external global i64, align 8
@__current_row = external global ptr, align 8
@str.5 = private unnamed_addr constant [5 x i8] c"date\00", align 1
@str.6 = private unnamed_addr constant [9 x i8] c"latitude\00", align 1
@str.7 = private unnamed_addr constant [10 x i8] c"longitude\00", align 1
@str.8 = private unnamed_addr constant [11 x i8] c"fire_label\00", align 1
@str.9 = private unnamed_addr constant [20 x i8] c"land_cover_class_17\00", align 1

define ptr @main_13() {
entry:
  %df2_load = load ptr, ptr @df2, align 8
  store ptr %df2_load, ptr @__map_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 64)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 5, ptr %len_ptr, align 8
  store i64 5, ptr %cap_ptr, align 8
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
  %__map_src_load = load ptr, ptr @__map_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  %total_size = mul i64 8, %rows_length
  %padded_size = add i64 %total_size, 31
  %aligned_size = and i64 %padded_size, -32
  %is_zero = icmp eq i64 %aligned_size, 0
  %final_malloc_size = select i1 %is_zero, i64 8, i64 %aligned_size
  %arr_header5 = call ptr @malloc(i64 24)
  %arr_data_raw6 = call ptr @malloc(i64 %final_malloc_size)
  %len_ptr7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 0
  %cap_ptr8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 1
  %data_field_ptr9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 2
  store i64 0, ptr %len_ptr7, align 8
  store i64 %rows_length, ptr %cap_ptr8, align 8
  store ptr %arr_data_raw6, ptr %data_field_ptr9, align 8
  %arr_header10 = call ptr @malloc(i64 24)
  %arr_data_raw11 = call ptr @malloc(i64 64)
  %len_ptr12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header10, i32 0, i32 0
  %cap_ptr13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header10, i32 0, i32 1
  %data_field_ptr14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header10, i32 0, i32 2
  store i64 5, ptr %len_ptr12, align 8
  store i64 5, ptr %cap_ptr13, align 8
  store ptr %arr_data_raw11, ptr %data_field_ptr14, align 8
  %elem_ptr15 = getelementptr i64, ptr %arr_data_raw11, i64 0
  store i64 4, ptr %elem_ptr15, align 8
  %elem_ptr16 = getelementptr i64, ptr %arr_data_raw11, i64 1
  store i64 2, ptr %elem_ptr16, align 8
  %elem_ptr17 = getelementptr i64, ptr %arr_data_raw11, i64 2
  store i64 2, ptr %elem_ptr17, align 8
  %elem_ptr18 = getelementptr i64, ptr %arr_data_raw11, i64 3
  store i64 1, ptr %elem_ptr18, align 8
  %elem_ptr19 = getelementptr i64, ptr %arr_data_raw11, i64 4
  store i64 3, ptr %elem_ptr19, align 8
  %df_header = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %col_ptr = getelementptr inbounds nuw %dataframe, ptr %df_header, i32 0, i32 0
  store ptr %arr_header, ptr %col_ptr, align 8
  %row_ptr = getelementptr inbounds nuw %dataframe, ptr %df_header, i32 0, i32 1
  store ptr %arr_header5, ptr %row_ptr, align 8
  %type_ptr = getelementptr inbounds nuw %dataframe, ptr %df_header, i32 0, i32 2
  store ptr %arr_header10, ptr %type_ptr, align 8
  store ptr %df_header, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__map_i_load = load i64, ptr @__map_i, align 8
  %__map_src_load20 = load ptr, ptr @__map_src, align 8
  %rows_ptr_field21 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load20, i32 0, i32 1
  %rows_ptr22 = load ptr, ptr %rows_ptr_field21, align 8
  %rows_array_ptr23 = bitcast ptr %rows_ptr22 to ptr
  %rows_length_ptr24 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr23, i32 0, i32 0
  %rows_length25 = load i64, ptr %rows_length_ptr24, align 8
  %icmp_tmp = icmp slt i64 %__map_i_load, %rows_length25
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_src_load26 = load ptr, ptr @__map_src, align 8
  %__map_i_load27 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load26, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %elem_ptr28 = getelementptr ptr, ptr %data, i64 %__map_i_load27
  %record = load ptr, ptr %elem_ptr28, align 8
  store ptr %record, ptr @__current_row, align 8
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %record_ptr = call ptr @malloc(i64 ptrtoint (ptr getelementptr ({ i64, i64, i64, i64, i64 }, ptr null, i32 1) to i64))
  %__current_row_load = load ptr, ptr @__current_row, align 8
  %ptr_date = getelementptr ptr, ptr %__current_row_load, i64 0
  %val_date = load ptr, ptr %ptr_date, align 8
  %ptr_to_i64 = ptrtoint ptr %val_date to i64
  %field_0 = getelementptr inbounds nuw { i64, i64, i64, i64, i64 }, ptr %record_ptr, i32 0, i32 0
  store i64 %ptr_to_i64, ptr %field_0, align 8
  %__current_row_load29 = load ptr, ptr @__current_row, align 8
  %ptr_latitude = getelementptr ptr, ptr %__current_row_load29, i64 1
  %val_latitude = load double, ptr %ptr_latitude, align 8
  %double_to_i64_bits = bitcast double %val_latitude to i64
  %field_1 = getelementptr inbounds nuw { i64, i64, i64, i64, i64 }, ptr %record_ptr, i32 0, i32 1
  store i64 %double_to_i64_bits, ptr %field_1, align 8
  %__current_row_load30 = load ptr, ptr @__current_row, align 8
  %ptr_longitude = getelementptr ptr, ptr %__current_row_load30, i64 2
  %val_longitude = load double, ptr %ptr_longitude, align 8
  %double_to_i64_bits31 = bitcast double %val_longitude to i64
  %field_2 = getelementptr inbounds nuw { i64, i64, i64, i64, i64 }, ptr %record_ptr, i32 0, i32 2
  store i64 %double_to_i64_bits31, ptr %field_2, align 8
  %__current_row_load32 = load ptr, ptr @__current_row, align 8
  %ptr_fire_label = getelementptr ptr, ptr %__current_row_load32, i64 3
  %val_fire_label = load i64, ptr %ptr_fire_label, align 4
  %field_3 = getelementptr inbounds nuw { i64, i64, i64, i64, i64 }, ptr %record_ptr, i32 0, i32 3
  store i64 %val_fire_label, ptr %field_3, align 8
  %__current_row_load33 = load ptr, ptr @__current_row, align 8
  %ptr_land_cover_class_17 = getelementptr ptr, ptr %__current_row_load33, i64 4
  %val_bool_raw = load i64, ptr %ptr_land_cover_class_17, align 4
  %val_land_cover_class_17 = trunc i64 %val_bool_raw to i1
  %bool_to_i64 = zext i1 %val_land_cover_class_17 to i64
  %field_4 = getelementptr inbounds nuw { i64, i64, i64, i64, i64 }, ptr %record_ptr, i32 0, i32 4
  store i64 %bool_to_i64, ptr %field_4, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr34 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr35 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr36 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len = load i64, ptr %len_ptr34, align 8
  %cap = load i64, ptr %cap_ptr35, align 8
  %data37 = load ptr, ptr %data_ptr_ptr36, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %cont

for.step:                                         ; preds = %cont
  %x_load = load i64, ptr @__map_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__map_result_load38 = load ptr, ptr @__map_result, align 8
  store ptr %__map_result_load38, ptr @__map_src, align 8
  %arr_header39 = call ptr @malloc(i64 24)
  %arr_data_raw40 = call ptr @malloc(i64 64)
  %len_ptr41 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 0
  %cap_ptr42 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 1
  %data_field_ptr43 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header39, i32 0, i32 2
  store i64 5, ptr %len_ptr41, align 8
  store i64 5, ptr %cap_ptr42, align 8
  store ptr %arr_data_raw40, ptr %data_field_ptr43, align 8
  %elem_ptr44 = getelementptr ptr, ptr %arr_data_raw40, i64 0
  store ptr @str.5, ptr %elem_ptr44, align 8
  %elem_ptr45 = getelementptr ptr, ptr %arr_data_raw40, i64 1
  store ptr @str.6, ptr %elem_ptr45, align 8
  %elem_ptr46 = getelementptr ptr, ptr %arr_data_raw40, i64 2
  store ptr @str.7, ptr %elem_ptr46, align 8
  %elem_ptr47 = getelementptr ptr, ptr %arr_data_raw40, i64 3
  store ptr @str.8, ptr %elem_ptr47, align 8
  %elem_ptr48 = getelementptr ptr, ptr %arr_data_raw40, i64 4
  store ptr @str.9, ptr %elem_ptr48, align 8
  %__map_src_load49 = load ptr, ptr @__map_src, align 8
  %rows_ptr_field50 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load49, i32 0, i32 1
  %rows_ptr51 = load ptr, ptr %rows_ptr_field50, align 8
  %rows_array_ptr52 = bitcast ptr %rows_ptr51 to ptr
  %rows_length_ptr53 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr52, i32 0, i32 0
  %rows_length54 = load i64, ptr %rows_length_ptr53, align 8
  %total_size55 = mul i64 8, %rows_length54
  %padded_size56 = add i64 %total_size55, 31
  %aligned_size57 = and i64 %padded_size56, -32
  %is_zero58 = icmp eq i64 %aligned_size57, 0
  %final_malloc_size59 = select i1 %is_zero58, i64 8, i64 %aligned_size57
  %arr_header60 = call ptr @malloc(i64 24)
  %arr_data_raw61 = call ptr @malloc(i64 %final_malloc_size59)
  %len_ptr62 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header60, i32 0, i32 0
  %cap_ptr63 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header60, i32 0, i32 1
  %data_field_ptr64 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header60, i32 0, i32 2
  store i64 0, ptr %len_ptr62, align 8
  store i64 %rows_length54, ptr %cap_ptr63, align 8
  store ptr %arr_data_raw61, ptr %data_field_ptr64, align 8
  %arr_header65 = call ptr @malloc(i64 24)
  %arr_data_raw66 = call ptr @malloc(i64 64)
  %len_ptr67 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header65, i32 0, i32 0
  %cap_ptr68 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header65, i32 0, i32 1
  %data_field_ptr69 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header65, i32 0, i32 2
  store i64 5, ptr %len_ptr67, align 8
  store i64 5, ptr %cap_ptr68, align 8
  store ptr %arr_data_raw66, ptr %data_field_ptr69, align 8
  %elem_ptr70 = getelementptr i64, ptr %arr_data_raw66, i64 0
  store i64 4, ptr %elem_ptr70, align 8
  %elem_ptr71 = getelementptr i64, ptr %arr_data_raw66, i64 1
  store i64 2, ptr %elem_ptr71, align 8
  %elem_ptr72 = getelementptr i64, ptr %arr_data_raw66, i64 2
  store i64 2, ptr %elem_ptr72, align 8
  %elem_ptr73 = getelementptr i64, ptr %arr_data_raw66, i64 3
  store i64 1, ptr %elem_ptr73, align 8
  %elem_ptr74 = getelementptr i64, ptr %arr_data_raw66, i64 4
  store i64 3, ptr %elem_ptr74, align 8
  %df_header75 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %col_ptr76 = getelementptr inbounds nuw %dataframe, ptr %df_header75, i32 0, i32 0
  store ptr %arr_header39, ptr %col_ptr76, align 8
  %row_ptr77 = getelementptr inbounds nuw %dataframe, ptr %df_header75, i32 0, i32 1
  store ptr %arr_header60, ptr %row_ptr77, align 8
  %type_ptr78 = getelementptr inbounds nuw %dataframe, ptr %df_header75, i32 0, i32 2
  store ptr %arr_header65, ptr %type_ptr78, align 8
  store ptr %df_header75, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond79

grow:                                             ; preds = %for.body
  %0 = icmp eq i64 %cap, 0
  %1 = mul i64 %cap, 2
  %new_cap = select i1 %0, i64 4, i64 %1
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data37, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr35, align 8
  store ptr %realloc, ptr %data_ptr_ptr36, align 8
  br label %cont

cont:                                             ; preds = %grow, %for.body
  %final_data_ptr = phi ptr [ %data37, %for.body ], [ %realloc, %grow ]
  %target_slot_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %record_ptr, ptr %target_slot_ptr, align 8
  %new_len = add i64 %len, 1
  store i64 %new_len, ptr %len_ptr34, align 8
  br label %for.step

for.cond79:                                       ; preds = %for.step81, %for.end
  %__map_i_load83 = load i64, ptr @__map_i, align 8
  %__map_src_load84 = load ptr, ptr @__map_src, align 8
  %rows_ptr_field85 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load84, i32 0, i32 1
  %rows_ptr86 = load ptr, ptr %rows_ptr_field85, align 8
  %rows_array_ptr87 = bitcast ptr %rows_ptr86 to ptr
  %rows_length_ptr88 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr87, i32 0, i32 0
  %rows_length89 = load i64, ptr %rows_length_ptr88, align 8
  %icmp_tmp90 = icmp slt i64 %__map_i_load83, %rows_length89
  br i1 %icmp_tmp90, label %for.body80, label %for.end82

for.body80:                                       ; preds = %for.cond79
  %__map_src_load91 = load ptr, ptr @__map_src, align 8
  %__map_i_load92 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr93 = getelementptr inbounds nuw %dataframe, ptr %__map_src_load91, i32 0, i32 1
  %rows94 = load ptr, ptr %rows_ptr_ptr93, align 8
  %data_ptr_ptr95 = getelementptr inbounds nuw %array, ptr %rows94, i32 0, i32 2
  %data96 = load ptr, ptr %data_ptr_ptr95, align 8
  %elem_ptr97 = getelementptr ptr, ptr %data96, i64 %__map_i_load92
  %record98 = load ptr, ptr %elem_ptr97, align 8
  store ptr %record98, ptr @__current_row, align 8
  %__map_result_load99 = load ptr, ptr @__map_result, align 8
  %record_ptr100 = call ptr @malloc(i64 ptrtoint (ptr getelementptr ({ i64, i64, i64, i64, i64 }, ptr null, i32 1) to i64))
  %__current_row_load101 = load ptr, ptr @__current_row, align 8
  %ptr_date102 = getelementptr ptr, ptr %__current_row_load101, i64 0
  %val_date103 = load ptr, ptr %ptr_date102, align 8
  %ptr_to_i64104 = ptrtoint ptr %val_date103 to i64
  %field_0105 = getelementptr inbounds nuw { i64, i64, i64, i64, i64 }, ptr %record_ptr100, i32 0, i32 0
  store i64 %ptr_to_i64104, ptr %field_0105, align 8
  %__current_row_load106 = load ptr, ptr @__current_row, align 8
  %ptr_latitude107 = getelementptr ptr, ptr %__current_row_load106, i64 1
  %val_latitude108 = load double, ptr %ptr_latitude107, align 8
  %double_to_i64_bits109 = bitcast double %val_latitude108 to i64
  %field_1110 = getelementptr inbounds nuw { i64, i64, i64, i64, i64 }, ptr %record_ptr100, i32 0, i32 1
  store i64 %double_to_i64_bits109, ptr %field_1110, align 8
  %__current_row_load111 = load ptr, ptr @__current_row, align 8
  %ptr_longitude112 = getelementptr ptr, ptr %__current_row_load111, i64 2
  %val_longitude113 = load double, ptr %ptr_longitude112, align 8
  %double_to_i64_bits114 = bitcast double %val_longitude113 to i64
  %field_2115 = getelementptr inbounds nuw { i64, i64, i64, i64, i64 }, ptr %record_ptr100, i32 0, i32 2
  store i64 %double_to_i64_bits114, ptr %field_2115, align 8
  %__current_row_load116 = load ptr, ptr @__current_row, align 8
  %ptr_fire_label117 = getelementptr ptr, ptr %__current_row_load116, i64 3
  %val_fire_label118 = load i64, ptr %ptr_fire_label117, align 4
  %field_3119 = getelementptr inbounds nuw { i64, i64, i64, i64, i64 }, ptr %record_ptr100, i32 0, i32 3
  store i64 %val_fire_label118, ptr %field_3119, align 8
  %__current_row_load120 = load ptr, ptr @__current_row, align 8
  %ptr_land_cover_class_17121 = getelementptr ptr, ptr %__current_row_load120, i64 4
  %val_bool_raw122 = load i64, ptr %ptr_land_cover_class_17121, align 4
  %val_land_cover_class_17123 = trunc i64 %val_bool_raw122 to i1
  %bool_to_i64124 = zext i1 %val_land_cover_class_17123 to i64
  %field_4125 = getelementptr inbounds nuw { i64, i64, i64, i64, i64 }, ptr %record_ptr100, i32 0, i32 4
  store i64 %bool_to_i64124, ptr %field_4125, align 8
  %rows_field126 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_result_load99, i32 0, i32 1
  %rows_array127 = load ptr, ptr %rows_field126, align 8
  %len_ptr128 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array127, i32 0, i32 0
  %cap_ptr129 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array127, i32 0, i32 1
  %data_ptr_ptr130 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array127, i32 0, i32 2
  %len131 = load i64, ptr %len_ptr128, align 8
  %cap132 = load i64, ptr %cap_ptr129, align 8
  %data133 = load ptr, ptr %data_ptr_ptr130, align 8
  %is_full134 = icmp uge i64 %len131, %cap132
  br i1 %is_full134, label %grow135, label %cont136

for.step81:                                       ; preds = %cont136
  %x_load143 = load i64, ptr @__map_i, align 8
  %inc_add144 = add i64 %x_load143, 1
  store i64 %inc_add144, ptr @__map_i, align 8
  br label %for.cond79, !llvm.loop !0

for.end82:                                        ; preds = %for.cond79
  %__map_result_load145 = load ptr, ptr @__map_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__map_result_load145, ptr %data_ptr, align 8
  ret ptr %runtime_obj

grow135:                                          ; preds = %for.body80
  %2 = icmp eq i64 %cap132, 0
  %3 = mul i64 %cap132, 2
  %new_cap137 = select i1 %2, i64 4, i64 %3
  %bytes138 = mul i64 %new_cap137, 8
  %realloc139 = call ptr @realloc(ptr %data133, i64 %bytes138)
  store i64 %new_cap137, ptr %cap_ptr129, align 8
  store ptr %realloc139, ptr %data_ptr_ptr130, align 8
  br label %cont136

cont136:                                          ; preds = %grow135, %for.body80
  %final_data_ptr140 = phi ptr [ %data133, %for.body80 ], [ %realloc139, %grow135 ]
  %target_slot_ptr141 = getelementptr ptr, ptr %final_data_ptr140, i64 %len131
  store ptr %record_ptr100, ptr %target_slot_ptr141, align 8
  %new_len142 = add i64 %len131, 1
  store i64 %new_len142, ptr %len_ptr128, align 8
  br label %for.step81
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i1 true}
