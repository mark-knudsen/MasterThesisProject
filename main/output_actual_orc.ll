; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i32, i32, ptr }
%struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17 = type { ptr, float, float, float, float, float, float, float, float, float, float, float, float, float, float, float, float, float, float, float, i32, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8 }

@df = external global ptr
@__map_src = global ptr null, align 8
@__map_result = global ptr null, align 8
@__map_i = global i32 0, align 8
@df_lat = global ptr null, align 8

define ptr @main_1() {
entry:
  %df_load = load ptr, ptr @df, align 8
  store ptr %df_load, ptr @__map_src, align 8
  %arr_header = call ptr @malloc(i32 16)
  %arr_data_raw = call ptr @malloc(i32 800)
  %len_ptr = getelementptr inbounds nuw { i32, i32, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i32, i32, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i32, i32, ptr }, ptr %arr_header, i32 0, i32 2
  store i32 0, ptr %len_ptr, align 4
  store i32 100, ptr %cap_ptr, align 4
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  store ptr %arr_header, ptr @__map_result, align 8
  store i32 0, ptr @__map_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__map_i_load = load i32, ptr @__map_i, align 8
  %__map_src_load = load ptr, ptr @__map_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i32, i32, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i32, ptr %rows_length_ptr, align 4
  %rows_length1 = zext i32 %rows_length to i64
  %icmp_tmp = icmp slt i32 %__map_i_load, i64 %rows_length1
  %fortest_int = icmp ne i1 %icmp_tmp, false
  br i1 %fortest_int, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %__map_src_load2 = load ptr, ptr @__map_src, align 8
  %__map_i_load3 = load i32, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load2, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %idx_cast = sext i32 %__map_i_load3 to i64
  %elem_ptr = getelementptr ptr, ptr %data, i64 %idx_cast
  %record = load ptr, ptr %elem_ptr, align 8
  %ptr_latitude = getelementptr %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %record, i32 0, i32 1
  %val_latitude = load double, ptr %ptr_latitude, align 8
  %len_ptr4 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__map_result_load, i32 0, i32 0
  %cap_ptr5 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__map_result_load, i32 0, i32 1
  %data_ptr_ptr6 = getelementptr inbounds nuw { i32, i32, ptr }, ptr %__map_result_load, i32 0, i32 2
  %len32 = load i32, ptr %len_ptr4, align 4
  %cap32 = load i32, ptr %cap_ptr5, align 4
  %data7 = load ptr, ptr %data_ptr_ptr6, align 8
  %is_full = icmp uge i32 %len32, %cap32
  br i1 %is_full, label %grow, label %cont

for.step:                                         ; preds = %cont
  %x_load = load i32, ptr @__map_i, align 4
  %inc_add = add i32 %x_load, 1
  store i32 %inc_add, ptr @__map_i, align 4
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__map_result_load8 = load ptr, ptr @__map_result, align 8
  store ptr %__map_result_load8, ptr @df_lat, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj

grow:                                             ; preds = %for.body
  %0 = icmp eq i32 %cap32, 0
  %1 = mul i32 %cap32, 2
  %new_cap = select i1 %0, i32 4, i32 %1
  %bytes = mul i32 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data7, i32 %bytes)
  store i32 %new_cap, ptr %cap_ptr5, align 4
  store ptr %realloc, ptr %data_ptr_ptr6, align 8
  br label %cont

cont:                                             ; preds = %grow, %for.body
  %data_phi = phi ptr [ %data7, %for.body ], [ %realloc, %grow ]
  %slot = getelementptr float, ptr %data_phi, i32 %len32
  store double %val_latitude, ptr %slot, align 4
  %new_len = add i32 %len32, 1
  store i32 %new_len, ptr %len_ptr4, align 4
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i32)

declare ptr @realloc(ptr, i32)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i8 1}
