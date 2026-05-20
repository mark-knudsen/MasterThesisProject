; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }
%struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17 = type { ptr, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, i64, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8 }

@df = external global ptr
@__col_src = external global ptr, align 8
@__col_len = external global i64, align 8
@__col_result = external global ptr, align 8
@__col_i = external global i64, align 8
@df_idx_err_msg = private unnamed_addr constant [50 x i8] c"Runtime Error: Dataframe row index out of bounds\0A\00", align 1
@__col_row = external global ptr, align 8

define ptr @main_6() {
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
  %arr_data_raw = call ptr @malloc(i64 128)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %len_ptr, align 8
  store i64 100, ptr %cap_ptr, align 8
  store ptr %arr_data_raw, ptr %data_field_ptr, align 8
  store ptr %arr_header, ptr @__col_result, align 8
  store i64 0, ptr @__col_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__col_i_load = load i64, ptr @__col_i, align 8
  %__col_len_load = load i64, ptr @__col_len, align 8
  %icmp_tmp = icmp slt i64 %__col_i_load, %__col_len_load
  %fortest_int = icmp ne i1 %icmp_tmp, false
  br i1 %fortest_int, label %for.body, label %for.end

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
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 5, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %__col_result_load9, ptr %data_ptr, align 8
  ret ptr %runtime_obj

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
  %ptr_land_cover_class_16 = getelementptr %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %__col_row_load, i32 0, i32 35
  %val_bool_raw = load i64, ptr %ptr_land_cover_class_16, align 4
  %val_land_cover_class_16 = trunc i64 %val_bool_raw to i8
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
  %bytes = mul i64 %new_cap, 1
  %realloc = call ptr @realloc(ptr %data8, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr5, align 8
  store ptr %realloc, ptr %data_ptr_ptr6, align 8
  br label %cont

cont:                                             ; preds = %grow, %df_idx_merge
  %final_data_ptr = phi ptr [ %data8, %df_idx_merge ], [ %realloc, %grow ]
  %target_slot_ptr = getelementptr i8, ptr %final_data_ptr, i64 %len7
  store i8 %val_land_cover_class_16, ptr %target_slot_ptr, align 8
  %new_len = add i64 %len7, 1
  store i64 %new_len, ptr %len_ptr4, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i8 1}
