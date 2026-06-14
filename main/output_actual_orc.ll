; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }
%struct_date = type { ptr }
%struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17 = type { ptr, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, i64, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8 }

@df = external global ptr
@__map_src = global ptr null, align 8
@str = private unnamed_addr constant [5 x i8] c"date\00", align 1
@__map_result = global ptr null, align 8
@__map_i = global i64 0, align 8
@__current_row = global ptr null, align 8

define ptr @main_2() {
entry:
  %df_load = load ptr, ptr @df, align 8
  store ptr %df_load, ptr @__map_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 32)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 1, ptr %len_ptr, align 8
  store i64 2, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data_raw, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %arr_header1 = call ptr @malloc(i64 24)
  %arr_data_raw2 = call ptr @malloc(i64 800)
  %len_ptr3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 0
  %cap_ptr4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 1
  %data_field_ptr5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 2
  store i64 0, ptr %len_ptr3, align 8
  store i64 100, ptr %cap_ptr4, align 8
  store ptr %arr_data_raw2, ptr %data_field_ptr5, align 8
  %arr_header6 = call ptr @malloc(i64 24)
  %arr_data_raw7 = call ptr @malloc(i64 32)
  %len_ptr8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header6, i32 0, i32 0
  %cap_ptr9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header6, i32 0, i32 1
  %data_field_ptr10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header6, i32 0, i32 2
  store i64 1, ptr %len_ptr8, align 8
  store i64 2, ptr %cap_ptr9, align 8
  store ptr %arr_data_raw7, ptr %data_field_ptr10, align 8
  %elem_ptr11 = getelementptr i64, ptr %arr_data_raw7, i64 0
  store i64 4, ptr %elem_ptr11, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %cols_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  %rows_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  %types_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header, ptr %cols_gep, align 8
  store ptr %arr_header1, ptr %rows_gep, align 8
  store ptr %arr_header6, ptr %types_gep, align 8
  store ptr %df, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__map_i_load = load i64, ptr @__map_i, align 8
  %__map_src_load = load ptr, ptr @__map_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %len_ptr12 = getelementptr inbounds nuw %array, ptr %rows_ptr, i32 0, i32 0
  %len = load i64, ptr %len_ptr12, align 8
  %icmp_tmp = icmp slt i64 %__map_i_load, %len
  %fortest_int = icmp ne i1 %icmp_tmp, false
  br i1 %fortest_int, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_src_load13 = load ptr, ptr @__map_src, align 8
  %__map_i_load14 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load13, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %elem_ptr15 = getelementptr ptr, ptr %data, i64 %__map_i_load14
  %record = load ptr, ptr %elem_ptr15, align 8
  store ptr %record, ptr @__current_row, align 8
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %record_ptr = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_date, ptr null, i32 1) to i64))
  %__current_row_load = load ptr, ptr @__current_row, align 8
  %ptr_date = getelementptr %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %__current_row_load, i32 0, i32 0
  %val_date = load ptr, ptr %ptr_date, align 8
  %field_0 = getelementptr %struct_date, ptr %record_ptr, i32 0, i32 0
  store ptr %val_date, ptr %field_0, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len19 = load i64, ptr %len_ptr16, align 8
  %cap = load i64, ptr %cap_ptr17, align 8
  %data20 = load ptr, ptr %data_ptr_ptr18, align 8
  %is_full = icmp uge i64 %len19, %cap
  br i1 %is_full, label %grow, label %cont

for.step:                                         ; preds = %cont
  %x_load = load i64, ptr @__map_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__map_result_load21 = load ptr, ptr @__map_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %__map_result_load21, ptr %data_ptr, align 8
  ret ptr %runtime_obj

grow:                                             ; preds = %for.body
  %0 = icmp eq i64 %cap, 0
  %1 = mul i64 %cap, 2
  %new_cap = select i1 %0, i64 4, i64 %1
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data20, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr17, align 8
  store ptr %realloc, ptr %data_ptr_ptr18, align 8
  br label %cont

cont:                                             ; preds = %grow, %for.body
  %data_phi = phi ptr [ %data20, %for.body ], [ %realloc, %grow ]
  %slot = getelementptr ptr, ptr %data_phi, i64 %len19
  store ptr %record_ptr, ptr %slot, align 8
  %new_len = add i64 %len19, 1
  store i64 %new_len, ptr %len_ptr16, align 8
  br label %for.step
}

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i8 1}
