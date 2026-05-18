; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }
%struct_latitude = type { double }
%struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17 = type { ptr, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, i64, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8 }
%struct_longitude = type { double }

@df = external global ptr
@__map_src = external global ptr, align 8
@str = private unnamed_addr constant [5 x i8] c"date\00", align 1
@str.1 = private unnamed_addr constant [9 x i8] c"latitude\00", align 1
@str.2 = private unnamed_addr constant [10 x i8] c"longitude\00", align 1
@str.3 = private unnamed_addr constant [15 x i8] c"wind-speed-min\00", align 1
@str.4 = private unnamed_addr constant [15 x i8] c"wind-speed-max\00", align 1
@str.5 = private unnamed_addr constant [16 x i8] c"wind-speed-mean\00", align 1
@str.6 = private unnamed_addr constant [19 x i8] c"wind-direction-min\00", align 1
@str.7 = private unnamed_addr constant [19 x i8] c"wind-direction-max\00", align 1
@str.8 = private unnamed_addr constant [20 x i8] c"wind-direction-mean\00", align 1
@str.9 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-min\00", align 1
@str.10 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-max\00", align 1
@str.11 = private unnamed_addr constant [29 x i8] c"surface-air-temperature-mean\00", align 1
@str.12 = private unnamed_addr constant [19 x i8] c"total-rainfall-sum\00", align 1
@str.13 = private unnamed_addr constant [21 x i8] c"surface-humidity-min\00", align 1
@str.14 = private unnamed_addr constant [21 x i8] c"surface-humidity-max\00", align 1
@str.15 = private unnamed_addr constant [22 x i8] c"surface-humidity-mean\00", align 1
@str.16 = private unnamed_addr constant [5 x i8] c"ndvi\00", align 1
@str.17 = private unnamed_addr constant [10 x i8] c"elevation\00", align 1
@str.18 = private unnamed_addr constant [6 x i8] c"slope\00", align 1
@str.19 = private unnamed_addr constant [7 x i8] c"aspect\00", align 1
@str.20 = private unnamed_addr constant [11 x i8] c"fire_label\00", align 1
@str.21 = private unnamed_addr constant [19 x i8] c"land_cover_class_1\00", align 1
@str.22 = private unnamed_addr constant [19 x i8] c"land_cover_class_2\00", align 1
@str.23 = private unnamed_addr constant [19 x i8] c"land_cover_class_4\00", align 1
@str.24 = private unnamed_addr constant [19 x i8] c"land_cover_class_5\00", align 1
@str.25 = private unnamed_addr constant [19 x i8] c"land_cover_class_6\00", align 1
@str.26 = private unnamed_addr constant [19 x i8] c"land_cover_class_7\00", align 1
@str.27 = private unnamed_addr constant [19 x i8] c"land_cover_class_8\00", align 1
@str.28 = private unnamed_addr constant [19 x i8] c"land_cover_class_9\00", align 1
@str.29 = private unnamed_addr constant [20 x i8] c"land_cover_class_10\00", align 1
@str.30 = private unnamed_addr constant [20 x i8] c"land_cover_class_11\00", align 1
@str.31 = private unnamed_addr constant [20 x i8] c"land_cover_class_12\00", align 1
@str.32 = private unnamed_addr constant [20 x i8] c"land_cover_class_13\00", align 1
@str.33 = private unnamed_addr constant [20 x i8] c"land_cover_class_14\00", align 1
@str.34 = private unnamed_addr constant [20 x i8] c"land_cover_class_15\00", align 1
@str.35 = private unnamed_addr constant [20 x i8] c"land_cover_class_16\00", align 1
@str.36 = private unnamed_addr constant [20 x i8] c"land_cover_class_17\00", align 1
@__map_result = external global ptr, align 8
@__map_i = external global i64, align 8
@__current_row = external global ptr, align 8
@str.37 = private unnamed_addr constant [5 x i8] c"date\00", align 1
@str.38 = private unnamed_addr constant [9 x i8] c"latitude\00", align 1
@str.39 = private unnamed_addr constant [10 x i8] c"longitude\00", align 1
@str.40 = private unnamed_addr constant [15 x i8] c"wind-speed-min\00", align 1
@str.41 = private unnamed_addr constant [15 x i8] c"wind-speed-max\00", align 1
@str.42 = private unnamed_addr constant [16 x i8] c"wind-speed-mean\00", align 1
@str.43 = private unnamed_addr constant [19 x i8] c"wind-direction-min\00", align 1
@str.44 = private unnamed_addr constant [19 x i8] c"wind-direction-max\00", align 1
@str.45 = private unnamed_addr constant [20 x i8] c"wind-direction-mean\00", align 1
@str.46 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-min\00", align 1
@str.47 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-max\00", align 1
@str.48 = private unnamed_addr constant [29 x i8] c"surface-air-temperature-mean\00", align 1
@str.49 = private unnamed_addr constant [19 x i8] c"total-rainfall-sum\00", align 1
@str.50 = private unnamed_addr constant [21 x i8] c"surface-humidity-min\00", align 1
@str.51 = private unnamed_addr constant [21 x i8] c"surface-humidity-max\00", align 1
@str.52 = private unnamed_addr constant [22 x i8] c"surface-humidity-mean\00", align 1
@str.53 = private unnamed_addr constant [5 x i8] c"ndvi\00", align 1
@str.54 = private unnamed_addr constant [10 x i8] c"elevation\00", align 1
@str.55 = private unnamed_addr constant [6 x i8] c"slope\00", align 1
@str.56 = private unnamed_addr constant [7 x i8] c"aspect\00", align 1
@str.57 = private unnamed_addr constant [11 x i8] c"fire_label\00", align 1
@str.58 = private unnamed_addr constant [19 x i8] c"land_cover_class_1\00", align 1
@str.59 = private unnamed_addr constant [19 x i8] c"land_cover_class_2\00", align 1
@str.60 = private unnamed_addr constant [19 x i8] c"land_cover_class_4\00", align 1
@str.61 = private unnamed_addr constant [19 x i8] c"land_cover_class_5\00", align 1
@str.62 = private unnamed_addr constant [19 x i8] c"land_cover_class_6\00", align 1
@str.63 = private unnamed_addr constant [19 x i8] c"land_cover_class_7\00", align 1
@str.64 = private unnamed_addr constant [19 x i8] c"land_cover_class_8\00", align 1
@str.65 = private unnamed_addr constant [19 x i8] c"land_cover_class_9\00", align 1
@str.66 = private unnamed_addr constant [20 x i8] c"land_cover_class_10\00", align 1
@str.67 = private unnamed_addr constant [20 x i8] c"land_cover_class_11\00", align 1
@str.68 = private unnamed_addr constant [20 x i8] c"land_cover_class_12\00", align 1
@str.69 = private unnamed_addr constant [20 x i8] c"land_cover_class_13\00", align 1
@str.70 = private unnamed_addr constant [20 x i8] c"land_cover_class_14\00", align 1
@str.71 = private unnamed_addr constant [20 x i8] c"land_cover_class_15\00", align 1
@str.72 = private unnamed_addr constant [20 x i8] c"land_cover_class_16\00", align 1
@str.73 = private unnamed_addr constant [20 x i8] c"land_cover_class_17\00", align 1

define ptr @main_3() {
entry:
  %df_load = load ptr, ptr @df, align 8
  store ptr %df_load, ptr @__map_src, align 8
  %arr_header = call ptr @malloc(i64 24)
  %arr_data_raw = call ptr @malloc(i64 320)
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 37, ptr %len_ptr, align 8
  store i64 37, ptr %cap_ptr, align 8
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
  %elem_ptr5 = getelementptr ptr, ptr %arr_data_raw, i64 5
  store ptr @str.5, ptr %elem_ptr5, align 8
  %elem_ptr6 = getelementptr ptr, ptr %arr_data_raw, i64 6
  store ptr @str.6, ptr %elem_ptr6, align 8
  %elem_ptr7 = getelementptr ptr, ptr %arr_data_raw, i64 7
  store ptr @str.7, ptr %elem_ptr7, align 8
  %elem_ptr8 = getelementptr ptr, ptr %arr_data_raw, i64 8
  store ptr @str.8, ptr %elem_ptr8, align 8
  %elem_ptr9 = getelementptr ptr, ptr %arr_data_raw, i64 9
  store ptr @str.9, ptr %elem_ptr9, align 8
  %elem_ptr10 = getelementptr ptr, ptr %arr_data_raw, i64 10
  store ptr @str.10, ptr %elem_ptr10, align 8
  %elem_ptr11 = getelementptr ptr, ptr %arr_data_raw, i64 11
  store ptr @str.11, ptr %elem_ptr11, align 8
  %elem_ptr12 = getelementptr ptr, ptr %arr_data_raw, i64 12
  store ptr @str.12, ptr %elem_ptr12, align 8
  %elem_ptr13 = getelementptr ptr, ptr %arr_data_raw, i64 13
  store ptr @str.13, ptr %elem_ptr13, align 8
  %elem_ptr14 = getelementptr ptr, ptr %arr_data_raw, i64 14
  store ptr @str.14, ptr %elem_ptr14, align 8
  %elem_ptr15 = getelementptr ptr, ptr %arr_data_raw, i64 15
  store ptr @str.15, ptr %elem_ptr15, align 8
  %elem_ptr16 = getelementptr ptr, ptr %arr_data_raw, i64 16
  store ptr @str.16, ptr %elem_ptr16, align 8
  %elem_ptr17 = getelementptr ptr, ptr %arr_data_raw, i64 17
  store ptr @str.17, ptr %elem_ptr17, align 8
  %elem_ptr18 = getelementptr ptr, ptr %arr_data_raw, i64 18
  store ptr @str.18, ptr %elem_ptr18, align 8
  %elem_ptr19 = getelementptr ptr, ptr %arr_data_raw, i64 19
  store ptr @str.19, ptr %elem_ptr19, align 8
  %elem_ptr20 = getelementptr ptr, ptr %arr_data_raw, i64 20
  store ptr @str.20, ptr %elem_ptr20, align 8
  %elem_ptr21 = getelementptr ptr, ptr %arr_data_raw, i64 21
  store ptr @str.21, ptr %elem_ptr21, align 8
  %elem_ptr22 = getelementptr ptr, ptr %arr_data_raw, i64 22
  store ptr @str.22, ptr %elem_ptr22, align 8
  %elem_ptr23 = getelementptr ptr, ptr %arr_data_raw, i64 23
  store ptr @str.23, ptr %elem_ptr23, align 8
  %elem_ptr24 = getelementptr ptr, ptr %arr_data_raw, i64 24
  store ptr @str.24, ptr %elem_ptr24, align 8
  %elem_ptr25 = getelementptr ptr, ptr %arr_data_raw, i64 25
  store ptr @str.25, ptr %elem_ptr25, align 8
  %elem_ptr26 = getelementptr ptr, ptr %arr_data_raw, i64 26
  store ptr @str.26, ptr %elem_ptr26, align 8
  %elem_ptr27 = getelementptr ptr, ptr %arr_data_raw, i64 27
  store ptr @str.27, ptr %elem_ptr27, align 8
  %elem_ptr28 = getelementptr ptr, ptr %arr_data_raw, i64 28
  store ptr @str.28, ptr %elem_ptr28, align 8
  %elem_ptr29 = getelementptr ptr, ptr %arr_data_raw, i64 29
  store ptr @str.29, ptr %elem_ptr29, align 8
  %elem_ptr30 = getelementptr ptr, ptr %arr_data_raw, i64 30
  store ptr @str.30, ptr %elem_ptr30, align 8
  %elem_ptr31 = getelementptr ptr, ptr %arr_data_raw, i64 31
  store ptr @str.31, ptr %elem_ptr31, align 8
  %elem_ptr32 = getelementptr ptr, ptr %arr_data_raw, i64 32
  store ptr @str.32, ptr %elem_ptr32, align 8
  %elem_ptr33 = getelementptr ptr, ptr %arr_data_raw, i64 33
  store ptr @str.33, ptr %elem_ptr33, align 8
  %elem_ptr34 = getelementptr ptr, ptr %arr_data_raw, i64 34
  store ptr @str.34, ptr %elem_ptr34, align 8
  %elem_ptr35 = getelementptr ptr, ptr %arr_data_raw, i64 35
  store ptr @str.35, ptr %elem_ptr35, align 8
  %elem_ptr36 = getelementptr ptr, ptr %arr_data_raw, i64 36
  store ptr @str.36, ptr %elem_ptr36, align 8
  %arr_header37 = call ptr @malloc(i64 24)
  %arr_data_raw38 = call ptr @malloc(i64 800)
  %len_ptr39 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 0
  %cap_ptr40 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 1
  %data_field_ptr41 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 2
  store i64 0, ptr %len_ptr39, align 8
  store i64 100, ptr %cap_ptr40, align 8
  store ptr %arr_data_raw38, ptr %data_field_ptr41, align 8
  %arr_header42 = call ptr @malloc(i64 24)
  %arr_data_raw43 = call ptr @malloc(i64 320)
  %len_ptr44 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header42, i32 0, i32 0
  %cap_ptr45 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header42, i32 0, i32 1
  %data_field_ptr46 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header42, i32 0, i32 2
  store i64 37, ptr %len_ptr44, align 8
  store i64 37, ptr %cap_ptr45, align 8
  store ptr %arr_data_raw43, ptr %data_field_ptr46, align 8
  %elem_ptr47 = getelementptr i64, ptr %arr_data_raw43, i64 0
  store i64 4, ptr %elem_ptr47, align 8
  %elem_ptr48 = getelementptr i64, ptr %arr_data_raw43, i64 1
  store i64 4, ptr %elem_ptr48, align 8
  %elem_ptr49 = getelementptr i64, ptr %arr_data_raw43, i64 2
  store i64 4, ptr %elem_ptr49, align 8
  %elem_ptr50 = getelementptr i64, ptr %arr_data_raw43, i64 3
  store i64 4, ptr %elem_ptr50, align 8
  %elem_ptr51 = getelementptr i64, ptr %arr_data_raw43, i64 4
  store i64 4, ptr %elem_ptr51, align 8
  %elem_ptr52 = getelementptr i64, ptr %arr_data_raw43, i64 5
  store i64 4, ptr %elem_ptr52, align 8
  %elem_ptr53 = getelementptr i64, ptr %arr_data_raw43, i64 6
  store i64 4, ptr %elem_ptr53, align 8
  %elem_ptr54 = getelementptr i64, ptr %arr_data_raw43, i64 7
  store i64 4, ptr %elem_ptr54, align 8
  %elem_ptr55 = getelementptr i64, ptr %arr_data_raw43, i64 8
  store i64 4, ptr %elem_ptr55, align 8
  %elem_ptr56 = getelementptr i64, ptr %arr_data_raw43, i64 9
  store i64 4, ptr %elem_ptr56, align 8
  %elem_ptr57 = getelementptr i64, ptr %arr_data_raw43, i64 10
  store i64 4, ptr %elem_ptr57, align 8
  %elem_ptr58 = getelementptr i64, ptr %arr_data_raw43, i64 11
  store i64 4, ptr %elem_ptr58, align 8
  %elem_ptr59 = getelementptr i64, ptr %arr_data_raw43, i64 12
  store i64 4, ptr %elem_ptr59, align 8
  %elem_ptr60 = getelementptr i64, ptr %arr_data_raw43, i64 13
  store i64 4, ptr %elem_ptr60, align 8
  %elem_ptr61 = getelementptr i64, ptr %arr_data_raw43, i64 14
  store i64 4, ptr %elem_ptr61, align 8
  %elem_ptr62 = getelementptr i64, ptr %arr_data_raw43, i64 15
  store i64 4, ptr %elem_ptr62, align 8
  %elem_ptr63 = getelementptr i64, ptr %arr_data_raw43, i64 16
  store i64 4, ptr %elem_ptr63, align 8
  %elem_ptr64 = getelementptr i64, ptr %arr_data_raw43, i64 17
  store i64 4, ptr %elem_ptr64, align 8
  %elem_ptr65 = getelementptr i64, ptr %arr_data_raw43, i64 18
  store i64 4, ptr %elem_ptr65, align 8
  %elem_ptr66 = getelementptr i64, ptr %arr_data_raw43, i64 19
  store i64 4, ptr %elem_ptr66, align 8
  %elem_ptr67 = getelementptr i64, ptr %arr_data_raw43, i64 20
  store i64 4, ptr %elem_ptr67, align 8
  %elem_ptr68 = getelementptr i64, ptr %arr_data_raw43, i64 21
  store i64 4, ptr %elem_ptr68, align 8
  %elem_ptr69 = getelementptr i64, ptr %arr_data_raw43, i64 22
  store i64 4, ptr %elem_ptr69, align 8
  %elem_ptr70 = getelementptr i64, ptr %arr_data_raw43, i64 23
  store i64 4, ptr %elem_ptr70, align 8
  %elem_ptr71 = getelementptr i64, ptr %arr_data_raw43, i64 24
  store i64 4, ptr %elem_ptr71, align 8
  %elem_ptr72 = getelementptr i64, ptr %arr_data_raw43, i64 25
  store i64 4, ptr %elem_ptr72, align 8
  %elem_ptr73 = getelementptr i64, ptr %arr_data_raw43, i64 26
  store i64 4, ptr %elem_ptr73, align 8
  %elem_ptr74 = getelementptr i64, ptr %arr_data_raw43, i64 27
  store i64 4, ptr %elem_ptr74, align 8
  %elem_ptr75 = getelementptr i64, ptr %arr_data_raw43, i64 28
  store i64 4, ptr %elem_ptr75, align 8
  %elem_ptr76 = getelementptr i64, ptr %arr_data_raw43, i64 29
  store i64 4, ptr %elem_ptr76, align 8
  %elem_ptr77 = getelementptr i64, ptr %arr_data_raw43, i64 30
  store i64 4, ptr %elem_ptr77, align 8
  %elem_ptr78 = getelementptr i64, ptr %arr_data_raw43, i64 31
  store i64 4, ptr %elem_ptr78, align 8
  %elem_ptr79 = getelementptr i64, ptr %arr_data_raw43, i64 32
  store i64 4, ptr %elem_ptr79, align 8
  %elem_ptr80 = getelementptr i64, ptr %arr_data_raw43, i64 33
  store i64 4, ptr %elem_ptr80, align 8
  %elem_ptr81 = getelementptr i64, ptr %arr_data_raw43, i64 34
  store i64 4, ptr %elem_ptr81, align 8
  %elem_ptr82 = getelementptr i64, ptr %arr_data_raw43, i64 35
  store i64 4, ptr %elem_ptr82, align 8
  %elem_ptr83 = getelementptr i64, ptr %arr_data_raw43, i64 36
  store i64 4, ptr %elem_ptr83, align 8
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %cols_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  %rows_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  %types_gep = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header, ptr %cols_gep, align 8
  store ptr %arr_header37, ptr %rows_gep, align 8
  store ptr %arr_header42, ptr %types_gep, align 8
  store ptr %df, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__map_i_load = load i64, ptr @__map_i, align 8
  %__map_src_load = load ptr, ptr @__map_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__map_i_load, %rows_length
  %fortest_int = icmp ne i1 %icmp_tmp, false
  br i1 %fortest_int, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_src_load84 = load ptr, ptr @__map_src, align 8
  %__map_i_load85 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load84, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %elem_ptr86 = getelementptr ptr, ptr %data, i64 %__map_i_load85
  %record = load ptr, ptr %elem_ptr86, align 8
  store ptr %record, ptr @__current_row, align 8
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %__current_row_load = load ptr, ptr @__current_row, align 8
  %record_ptr = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_latitude, ptr null, i32 1) to i64))
  %__current_row_load87 = load ptr, ptr @__current_row, align 8
  %ptr_latitude = getelementptr %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %__current_row_load87, i32 0, i32 1
  %val_latitude = load double, ptr %ptr_latitude, align 8
  %fsubtmp = fsub double %val_latitude, 1.000000e+02
  %field_0 = getelementptr %struct_latitude, ptr %record_ptr, i32 0, i32 0
  store double %fsubtmp, ptr %field_0, align 8
  %__current_row_load88 = load ptr, ptr @__current_row, align 8
  %record_ptr89 = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_latitude, ptr null, i32 1) to i64))
  %__current_row_load90 = load ptr, ptr @__current_row, align 8
  %ptr_latitude91 = getelementptr %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %__current_row_load90, i32 0, i32 1
  %val_latitude92 = load double, ptr %ptr_latitude91, align 8
  %fsubtmp93 = fsub double %val_latitude92, 1.000000e+02
  %field_094 = getelementptr %struct_latitude, ptr %record_ptr89, i32 0, i32 0
  store double %fsubtmp93, ptr %field_094, align 8
  %0 = getelementptr ptr, ptr %__current_row_load88, i64 0
  %loaded_field = load ptr, ptr %0, align 8
  %1 = getelementptr ptr, ptr %record_ptr89, i64 0
  %loaded_field95 = load ptr, ptr %1, align 8
  %2 = getelementptr ptr, ptr %__current_row_load88, i64 2
  %loaded_field96 = load ptr, ptr %2, align 8
  %3 = getelementptr ptr, ptr %__current_row_load88, i64 3
  %loaded_field97 = load ptr, ptr %3, align 8
  %4 = getelementptr ptr, ptr %__current_row_load88, i64 4
  %loaded_field98 = load ptr, ptr %4, align 8
  %5 = getelementptr ptr, ptr %__current_row_load88, i64 5
  %loaded_field99 = load ptr, ptr %5, align 8
  %6 = getelementptr ptr, ptr %__current_row_load88, i64 6
  %loaded_field100 = load ptr, ptr %6, align 8
  %7 = getelementptr ptr, ptr %__current_row_load88, i64 7
  %loaded_field101 = load ptr, ptr %7, align 8
  %8 = getelementptr ptr, ptr %__current_row_load88, i64 8
  %loaded_field102 = load ptr, ptr %8, align 8
  %9 = getelementptr ptr, ptr %__current_row_load88, i64 9
  %loaded_field103 = load ptr, ptr %9, align 8
  %10 = getelementptr ptr, ptr %__current_row_load88, i64 10
  %loaded_field104 = load ptr, ptr %10, align 8
  %11 = getelementptr ptr, ptr %__current_row_load88, i64 11
  %loaded_field105 = load ptr, ptr %11, align 8
  %12 = getelementptr ptr, ptr %__current_row_load88, i64 12
  %loaded_field106 = load ptr, ptr %12, align 8
  %13 = getelementptr ptr, ptr %__current_row_load88, i64 13
  %loaded_field107 = load ptr, ptr %13, align 8
  %14 = getelementptr ptr, ptr %__current_row_load88, i64 14
  %loaded_field108 = load ptr, ptr %14, align 8
  %15 = getelementptr ptr, ptr %__current_row_load88, i64 15
  %loaded_field109 = load ptr, ptr %15, align 8
  %16 = getelementptr ptr, ptr %__current_row_load88, i64 16
  %loaded_field110 = load ptr, ptr %16, align 8
  %17 = getelementptr ptr, ptr %__current_row_load88, i64 17
  %loaded_field111 = load ptr, ptr %17, align 8
  %18 = getelementptr ptr, ptr %__current_row_load88, i64 18
  %loaded_field112 = load ptr, ptr %18, align 8
  %19 = getelementptr ptr, ptr %__current_row_load88, i64 19
  %loaded_field113 = load ptr, ptr %19, align 8
  %20 = getelementptr ptr, ptr %__current_row_load88, i64 20
  %loaded_field114 = load ptr, ptr %20, align 8
  %21 = getelementptr ptr, ptr %__current_row_load88, i64 21
  %loaded_field115 = load ptr, ptr %21, align 8
  %22 = getelementptr ptr, ptr %__current_row_load88, i64 22
  %loaded_field116 = load ptr, ptr %22, align 8
  %23 = getelementptr ptr, ptr %__current_row_load88, i64 23
  %loaded_field117 = load ptr, ptr %23, align 8
  %24 = getelementptr ptr, ptr %__current_row_load88, i64 24
  %loaded_field118 = load ptr, ptr %24, align 8
  %25 = getelementptr ptr, ptr %__current_row_load88, i64 25
  %loaded_field119 = load ptr, ptr %25, align 8
  %26 = getelementptr ptr, ptr %__current_row_load88, i64 26
  %loaded_field120 = load ptr, ptr %26, align 8
  %27 = getelementptr ptr, ptr %__current_row_load88, i64 27
  %loaded_field121 = load ptr, ptr %27, align 8
  %28 = getelementptr ptr, ptr %__current_row_load88, i64 28
  %loaded_field122 = load ptr, ptr %28, align 8
  %29 = getelementptr ptr, ptr %__current_row_load88, i64 29
  %loaded_field123 = load ptr, ptr %29, align 8
  %30 = getelementptr ptr, ptr %__current_row_load88, i64 30
  %loaded_field124 = load ptr, ptr %30, align 8
  %31 = getelementptr ptr, ptr %__current_row_load88, i64 31
  %loaded_field125 = load ptr, ptr %31, align 8
  %32 = getelementptr ptr, ptr %__current_row_load88, i64 32
  %loaded_field126 = load ptr, ptr %32, align 8
  %33 = getelementptr ptr, ptr %__current_row_load88, i64 33
  %loaded_field127 = load ptr, ptr %33, align 8
  %34 = getelementptr ptr, ptr %__current_row_load88, i64 34
  %loaded_field128 = load ptr, ptr %34, align 8
  %35 = getelementptr ptr, ptr %__current_row_load88, i64 35
  %loaded_field129 = load ptr, ptr %35, align 8
  %36 = getelementptr ptr, ptr %__current_row_load88, i64 36
  %loaded_field130 = load ptr, ptr %36, align 8
  %rec_ptr = call ptr @malloc(i64 296)
  %slot_0 = getelementptr ptr, ptr %rec_ptr, i64 0
  store ptr %loaded_field, ptr %slot_0, align 8
  %slot_1 = getelementptr ptr, ptr %rec_ptr, i64 1
  store ptr %loaded_field95, ptr %slot_1, align 8
  %slot_2 = getelementptr ptr, ptr %rec_ptr, i64 2
  store ptr %loaded_field96, ptr %slot_2, align 8
  %slot_3 = getelementptr ptr, ptr %rec_ptr, i64 3
  store ptr %loaded_field97, ptr %slot_3, align 8
  %slot_4 = getelementptr ptr, ptr %rec_ptr, i64 4
  store ptr %loaded_field98, ptr %slot_4, align 8
  %slot_5 = getelementptr ptr, ptr %rec_ptr, i64 5
  store ptr %loaded_field99, ptr %slot_5, align 8
  %slot_6 = getelementptr ptr, ptr %rec_ptr, i64 6
  store ptr %loaded_field100, ptr %slot_6, align 8
  %slot_7 = getelementptr ptr, ptr %rec_ptr, i64 7
  store ptr %loaded_field101, ptr %slot_7, align 8
  %slot_8 = getelementptr ptr, ptr %rec_ptr, i64 8
  store ptr %loaded_field102, ptr %slot_8, align 8
  %slot_9 = getelementptr ptr, ptr %rec_ptr, i64 9
  store ptr %loaded_field103, ptr %slot_9, align 8
  %slot_10 = getelementptr ptr, ptr %rec_ptr, i64 10
  store ptr %loaded_field104, ptr %slot_10, align 8
  %slot_11 = getelementptr ptr, ptr %rec_ptr, i64 11
  store ptr %loaded_field105, ptr %slot_11, align 8
  %slot_12 = getelementptr ptr, ptr %rec_ptr, i64 12
  store ptr %loaded_field106, ptr %slot_12, align 8
  %slot_13 = getelementptr ptr, ptr %rec_ptr, i64 13
  store ptr %loaded_field107, ptr %slot_13, align 8
  %slot_14 = getelementptr ptr, ptr %rec_ptr, i64 14
  store ptr %loaded_field108, ptr %slot_14, align 8
  %slot_15 = getelementptr ptr, ptr %rec_ptr, i64 15
  store ptr %loaded_field109, ptr %slot_15, align 8
  %slot_16 = getelementptr ptr, ptr %rec_ptr, i64 16
  store ptr %loaded_field110, ptr %slot_16, align 8
  %slot_17 = getelementptr ptr, ptr %rec_ptr, i64 17
  store ptr %loaded_field111, ptr %slot_17, align 8
  %slot_18 = getelementptr ptr, ptr %rec_ptr, i64 18
  store ptr %loaded_field112, ptr %slot_18, align 8
  %slot_19 = getelementptr ptr, ptr %rec_ptr, i64 19
  store ptr %loaded_field113, ptr %slot_19, align 8
  %slot_20 = getelementptr ptr, ptr %rec_ptr, i64 20
  store ptr %loaded_field114, ptr %slot_20, align 8
  %slot_21 = getelementptr ptr, ptr %rec_ptr, i64 21
  store ptr %loaded_field115, ptr %slot_21, align 8
  %slot_22 = getelementptr ptr, ptr %rec_ptr, i64 22
  store ptr %loaded_field116, ptr %slot_22, align 8
  %slot_23 = getelementptr ptr, ptr %rec_ptr, i64 23
  store ptr %loaded_field117, ptr %slot_23, align 8
  %slot_24 = getelementptr ptr, ptr %rec_ptr, i64 24
  store ptr %loaded_field118, ptr %slot_24, align 8
  %slot_25 = getelementptr ptr, ptr %rec_ptr, i64 25
  store ptr %loaded_field119, ptr %slot_25, align 8
  %slot_26 = getelementptr ptr, ptr %rec_ptr, i64 26
  store ptr %loaded_field120, ptr %slot_26, align 8
  %slot_27 = getelementptr ptr, ptr %rec_ptr, i64 27
  store ptr %loaded_field121, ptr %slot_27, align 8
  %slot_28 = getelementptr ptr, ptr %rec_ptr, i64 28
  store ptr %loaded_field122, ptr %slot_28, align 8
  %slot_29 = getelementptr ptr, ptr %rec_ptr, i64 29
  store ptr %loaded_field123, ptr %slot_29, align 8
  %slot_30 = getelementptr ptr, ptr %rec_ptr, i64 30
  store ptr %loaded_field124, ptr %slot_30, align 8
  %slot_31 = getelementptr ptr, ptr %rec_ptr, i64 31
  store ptr %loaded_field125, ptr %slot_31, align 8
  %slot_32 = getelementptr ptr, ptr %rec_ptr, i64 32
  store ptr %loaded_field126, ptr %slot_32, align 8
  %slot_33 = getelementptr ptr, ptr %rec_ptr, i64 33
  store ptr %loaded_field127, ptr %slot_33, align 8
  %slot_34 = getelementptr ptr, ptr %rec_ptr, i64 34
  store ptr %loaded_field128, ptr %slot_34, align 8
  %slot_35 = getelementptr ptr, ptr %rec_ptr, i64 35
  store ptr %loaded_field129, ptr %slot_35, align 8
  %slot_36 = getelementptr ptr, ptr %rec_ptr, i64 36
  store ptr %loaded_field130, ptr %slot_36, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr131 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr132 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr133 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len = load i64, ptr %len_ptr131, align 8
  %cap = load i64, ptr %cap_ptr132, align 8
  %data134 = load ptr, ptr %data_ptr_ptr133, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %cont

for.step:                                         ; preds = %cont
  %x_load = load i64, ptr @__map_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__map_result_load135 = load ptr, ptr @__map_result, align 8
  store ptr %__map_result_load135, ptr @__map_src, align 8
  %arr_header136 = call ptr @malloc(i64 24)
  %arr_data_raw137 = call ptr @malloc(i64 320)
  %len_ptr138 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header136, i32 0, i32 0
  %cap_ptr139 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header136, i32 0, i32 1
  %data_field_ptr140 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header136, i32 0, i32 2
  store i64 37, ptr %len_ptr138, align 8
  store i64 37, ptr %cap_ptr139, align 8
  store ptr %arr_data_raw137, ptr %data_field_ptr140, align 8
  %elem_ptr141 = getelementptr ptr, ptr %arr_data_raw137, i64 0
  store ptr @str.37, ptr %elem_ptr141, align 8
  %elem_ptr142 = getelementptr ptr, ptr %arr_data_raw137, i64 1
  store ptr @str.38, ptr %elem_ptr142, align 8
  %elem_ptr143 = getelementptr ptr, ptr %arr_data_raw137, i64 2
  store ptr @str.39, ptr %elem_ptr143, align 8
  %elem_ptr144 = getelementptr ptr, ptr %arr_data_raw137, i64 3
  store ptr @str.40, ptr %elem_ptr144, align 8
  %elem_ptr145 = getelementptr ptr, ptr %arr_data_raw137, i64 4
  store ptr @str.41, ptr %elem_ptr145, align 8
  %elem_ptr146 = getelementptr ptr, ptr %arr_data_raw137, i64 5
  store ptr @str.42, ptr %elem_ptr146, align 8
  %elem_ptr147 = getelementptr ptr, ptr %arr_data_raw137, i64 6
  store ptr @str.43, ptr %elem_ptr147, align 8
  %elem_ptr148 = getelementptr ptr, ptr %arr_data_raw137, i64 7
  store ptr @str.44, ptr %elem_ptr148, align 8
  %elem_ptr149 = getelementptr ptr, ptr %arr_data_raw137, i64 8
  store ptr @str.45, ptr %elem_ptr149, align 8
  %elem_ptr150 = getelementptr ptr, ptr %arr_data_raw137, i64 9
  store ptr @str.46, ptr %elem_ptr150, align 8
  %elem_ptr151 = getelementptr ptr, ptr %arr_data_raw137, i64 10
  store ptr @str.47, ptr %elem_ptr151, align 8
  %elem_ptr152 = getelementptr ptr, ptr %arr_data_raw137, i64 11
  store ptr @str.48, ptr %elem_ptr152, align 8
  %elem_ptr153 = getelementptr ptr, ptr %arr_data_raw137, i64 12
  store ptr @str.49, ptr %elem_ptr153, align 8
  %elem_ptr154 = getelementptr ptr, ptr %arr_data_raw137, i64 13
  store ptr @str.50, ptr %elem_ptr154, align 8
  %elem_ptr155 = getelementptr ptr, ptr %arr_data_raw137, i64 14
  store ptr @str.51, ptr %elem_ptr155, align 8
  %elem_ptr156 = getelementptr ptr, ptr %arr_data_raw137, i64 15
  store ptr @str.52, ptr %elem_ptr156, align 8
  %elem_ptr157 = getelementptr ptr, ptr %arr_data_raw137, i64 16
  store ptr @str.53, ptr %elem_ptr157, align 8
  %elem_ptr158 = getelementptr ptr, ptr %arr_data_raw137, i64 17
  store ptr @str.54, ptr %elem_ptr158, align 8
  %elem_ptr159 = getelementptr ptr, ptr %arr_data_raw137, i64 18
  store ptr @str.55, ptr %elem_ptr159, align 8
  %elem_ptr160 = getelementptr ptr, ptr %arr_data_raw137, i64 19
  store ptr @str.56, ptr %elem_ptr160, align 8
  %elem_ptr161 = getelementptr ptr, ptr %arr_data_raw137, i64 20
  store ptr @str.57, ptr %elem_ptr161, align 8
  %elem_ptr162 = getelementptr ptr, ptr %arr_data_raw137, i64 21
  store ptr @str.58, ptr %elem_ptr162, align 8
  %elem_ptr163 = getelementptr ptr, ptr %arr_data_raw137, i64 22
  store ptr @str.59, ptr %elem_ptr163, align 8
  %elem_ptr164 = getelementptr ptr, ptr %arr_data_raw137, i64 23
  store ptr @str.60, ptr %elem_ptr164, align 8
  %elem_ptr165 = getelementptr ptr, ptr %arr_data_raw137, i64 24
  store ptr @str.61, ptr %elem_ptr165, align 8
  %elem_ptr166 = getelementptr ptr, ptr %arr_data_raw137, i64 25
  store ptr @str.62, ptr %elem_ptr166, align 8
  %elem_ptr167 = getelementptr ptr, ptr %arr_data_raw137, i64 26
  store ptr @str.63, ptr %elem_ptr167, align 8
  %elem_ptr168 = getelementptr ptr, ptr %arr_data_raw137, i64 27
  store ptr @str.64, ptr %elem_ptr168, align 8
  %elem_ptr169 = getelementptr ptr, ptr %arr_data_raw137, i64 28
  store ptr @str.65, ptr %elem_ptr169, align 8
  %elem_ptr170 = getelementptr ptr, ptr %arr_data_raw137, i64 29
  store ptr @str.66, ptr %elem_ptr170, align 8
  %elem_ptr171 = getelementptr ptr, ptr %arr_data_raw137, i64 30
  store ptr @str.67, ptr %elem_ptr171, align 8
  %elem_ptr172 = getelementptr ptr, ptr %arr_data_raw137, i64 31
  store ptr @str.68, ptr %elem_ptr172, align 8
  %elem_ptr173 = getelementptr ptr, ptr %arr_data_raw137, i64 32
  store ptr @str.69, ptr %elem_ptr173, align 8
  %elem_ptr174 = getelementptr ptr, ptr %arr_data_raw137, i64 33
  store ptr @str.70, ptr %elem_ptr174, align 8
  %elem_ptr175 = getelementptr ptr, ptr %arr_data_raw137, i64 34
  store ptr @str.71, ptr %elem_ptr175, align 8
  %elem_ptr176 = getelementptr ptr, ptr %arr_data_raw137, i64 35
  store ptr @str.72, ptr %elem_ptr176, align 8
  %elem_ptr177 = getelementptr ptr, ptr %arr_data_raw137, i64 36
  store ptr @str.73, ptr %elem_ptr177, align 8
  %arr_header178 = call ptr @malloc(i64 24)
  %arr_data_raw179 = call ptr @malloc(i64 800)
  %len_ptr180 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header178, i32 0, i32 0
  %cap_ptr181 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header178, i32 0, i32 1
  %data_field_ptr182 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header178, i32 0, i32 2
  store i64 0, ptr %len_ptr180, align 8
  store i64 100, ptr %cap_ptr181, align 8
  store ptr %arr_data_raw179, ptr %data_field_ptr182, align 8
  %arr_header183 = call ptr @malloc(i64 24)
  %arr_data_raw184 = call ptr @malloc(i64 320)
  %len_ptr185 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header183, i32 0, i32 0
  %cap_ptr186 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header183, i32 0, i32 1
  %data_field_ptr187 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header183, i32 0, i32 2
  store i64 37, ptr %len_ptr185, align 8
  store i64 37, ptr %cap_ptr186, align 8
  store ptr %arr_data_raw184, ptr %data_field_ptr187, align 8
  %elem_ptr188 = getelementptr i64, ptr %arr_data_raw184, i64 0
  store i64 4, ptr %elem_ptr188, align 8
  %elem_ptr189 = getelementptr i64, ptr %arr_data_raw184, i64 1
  store i64 4, ptr %elem_ptr189, align 8
  %elem_ptr190 = getelementptr i64, ptr %arr_data_raw184, i64 2
  store i64 4, ptr %elem_ptr190, align 8
  %elem_ptr191 = getelementptr i64, ptr %arr_data_raw184, i64 3
  store i64 4, ptr %elem_ptr191, align 8
  %elem_ptr192 = getelementptr i64, ptr %arr_data_raw184, i64 4
  store i64 4, ptr %elem_ptr192, align 8
  %elem_ptr193 = getelementptr i64, ptr %arr_data_raw184, i64 5
  store i64 4, ptr %elem_ptr193, align 8
  %elem_ptr194 = getelementptr i64, ptr %arr_data_raw184, i64 6
  store i64 4, ptr %elem_ptr194, align 8
  %elem_ptr195 = getelementptr i64, ptr %arr_data_raw184, i64 7
  store i64 4, ptr %elem_ptr195, align 8
  %elem_ptr196 = getelementptr i64, ptr %arr_data_raw184, i64 8
  store i64 4, ptr %elem_ptr196, align 8
  %elem_ptr197 = getelementptr i64, ptr %arr_data_raw184, i64 9
  store i64 4, ptr %elem_ptr197, align 8
  %elem_ptr198 = getelementptr i64, ptr %arr_data_raw184, i64 10
  store i64 4, ptr %elem_ptr198, align 8
  %elem_ptr199 = getelementptr i64, ptr %arr_data_raw184, i64 11
  store i64 4, ptr %elem_ptr199, align 8
  %elem_ptr200 = getelementptr i64, ptr %arr_data_raw184, i64 12
  store i64 4, ptr %elem_ptr200, align 8
  %elem_ptr201 = getelementptr i64, ptr %arr_data_raw184, i64 13
  store i64 4, ptr %elem_ptr201, align 8
  %elem_ptr202 = getelementptr i64, ptr %arr_data_raw184, i64 14
  store i64 4, ptr %elem_ptr202, align 8
  %elem_ptr203 = getelementptr i64, ptr %arr_data_raw184, i64 15
  store i64 4, ptr %elem_ptr203, align 8
  %elem_ptr204 = getelementptr i64, ptr %arr_data_raw184, i64 16
  store i64 4, ptr %elem_ptr204, align 8
  %elem_ptr205 = getelementptr i64, ptr %arr_data_raw184, i64 17
  store i64 4, ptr %elem_ptr205, align 8
  %elem_ptr206 = getelementptr i64, ptr %arr_data_raw184, i64 18
  store i64 4, ptr %elem_ptr206, align 8
  %elem_ptr207 = getelementptr i64, ptr %arr_data_raw184, i64 19
  store i64 4, ptr %elem_ptr207, align 8
  %elem_ptr208 = getelementptr i64, ptr %arr_data_raw184, i64 20
  store i64 4, ptr %elem_ptr208, align 8
  %elem_ptr209 = getelementptr i64, ptr %arr_data_raw184, i64 21
  store i64 4, ptr %elem_ptr209, align 8
  %elem_ptr210 = getelementptr i64, ptr %arr_data_raw184, i64 22
  store i64 4, ptr %elem_ptr210, align 8
  %elem_ptr211 = getelementptr i64, ptr %arr_data_raw184, i64 23
  store i64 4, ptr %elem_ptr211, align 8
  %elem_ptr212 = getelementptr i64, ptr %arr_data_raw184, i64 24
  store i64 4, ptr %elem_ptr212, align 8
  %elem_ptr213 = getelementptr i64, ptr %arr_data_raw184, i64 25
  store i64 4, ptr %elem_ptr213, align 8
  %elem_ptr214 = getelementptr i64, ptr %arr_data_raw184, i64 26
  store i64 4, ptr %elem_ptr214, align 8
  %elem_ptr215 = getelementptr i64, ptr %arr_data_raw184, i64 27
  store i64 4, ptr %elem_ptr215, align 8
  %elem_ptr216 = getelementptr i64, ptr %arr_data_raw184, i64 28
  store i64 4, ptr %elem_ptr216, align 8
  %elem_ptr217 = getelementptr i64, ptr %arr_data_raw184, i64 29
  store i64 4, ptr %elem_ptr217, align 8
  %elem_ptr218 = getelementptr i64, ptr %arr_data_raw184, i64 30
  store i64 4, ptr %elem_ptr218, align 8
  %elem_ptr219 = getelementptr i64, ptr %arr_data_raw184, i64 31
  store i64 4, ptr %elem_ptr219, align 8
  %elem_ptr220 = getelementptr i64, ptr %arr_data_raw184, i64 32
  store i64 4, ptr %elem_ptr220, align 8
  %elem_ptr221 = getelementptr i64, ptr %arr_data_raw184, i64 33
  store i64 4, ptr %elem_ptr221, align 8
  %elem_ptr222 = getelementptr i64, ptr %arr_data_raw184, i64 34
  store i64 4, ptr %elem_ptr222, align 8
  %elem_ptr223 = getelementptr i64, ptr %arr_data_raw184, i64 35
  store i64 4, ptr %elem_ptr223, align 8
  %elem_ptr224 = getelementptr i64, ptr %arr_data_raw184, i64 36
  store i64 4, ptr %elem_ptr224, align 8
  %df225 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %cols_gep226 = getelementptr inbounds nuw %dataframe, ptr %df225, i32 0, i32 0
  %rows_gep227 = getelementptr inbounds nuw %dataframe, ptr %df225, i32 0, i32 1
  %types_gep228 = getelementptr inbounds nuw %dataframe, ptr %df225, i32 0, i32 2
  store ptr %arr_header136, ptr %cols_gep226, align 8
  store ptr %arr_header178, ptr %rows_gep227, align 8
  store ptr %arr_header183, ptr %types_gep228, align 8
  store ptr %df225, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond229

grow:                                             ; preds = %for.body
  %37 = icmp eq i64 %cap, 0
  %38 = mul i64 %cap, 2
  %new_cap = select i1 %37, i64 4, i64 %38
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data134, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr132, align 8
  store ptr %realloc, ptr %data_ptr_ptr133, align 8
  br label %cont

cont:                                             ; preds = %grow, %for.body
  %final_data_ptr = phi ptr [ %data134, %for.body ], [ %realloc, %grow ]
  %target_slot_ptr = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %rec_ptr, ptr %target_slot_ptr, align 8
  %new_len = add i64 %len, 1
  store i64 %new_len, ptr %len_ptr131, align 8
  br label %for.step

for.cond229:                                      ; preds = %for.step231, %for.end
  %__map_i_load233 = load i64, ptr @__map_i, align 8
  %__map_src_load234 = load ptr, ptr @__map_src, align 8
  %rows_ptr_field235 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load234, i32 0, i32 1
  %rows_ptr236 = load ptr, ptr %rows_ptr_field235, align 8
  %rows_array_ptr237 = bitcast ptr %rows_ptr236 to ptr
  %rows_length_ptr238 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr237, i32 0, i32 0
  %rows_length239 = load i64, ptr %rows_length_ptr238, align 8
  %icmp_tmp240 = icmp slt i64 %__map_i_load233, %rows_length239
  %fortest_int241 = icmp ne i1 %icmp_tmp240, false
  br i1 %fortest_int241, label %for.body230, label %for.end232

for.body230:                                      ; preds = %for.cond229
  %__map_src_load242 = load ptr, ptr @__map_src, align 8
  %__map_i_load243 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr244 = getelementptr inbounds nuw %dataframe, ptr %__map_src_load242, i32 0, i32 1
  %rows245 = load ptr, ptr %rows_ptr_ptr244, align 8
  %data_ptr_ptr246 = getelementptr inbounds nuw %array, ptr %rows245, i32 0, i32 2
  %data247 = load ptr, ptr %data_ptr_ptr246, align 8
  %elem_ptr248 = getelementptr ptr, ptr %data247, i64 %__map_i_load243
  %record249 = load ptr, ptr %elem_ptr248, align 8
  store ptr %record249, ptr @__current_row, align 8
  %__map_result_load250 = load ptr, ptr @__map_result, align 8
  %__current_row_load251 = load ptr, ptr @__current_row, align 8
  %record_ptr252 = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_longitude, ptr null, i32 1) to i64))
  %field_0253 = getelementptr %struct_longitude, ptr %record_ptr252, i32 0, i32 0
  store double 1.000000e+02, ptr %field_0253, align 8
  %__current_row_load254 = load ptr, ptr @__current_row, align 8
  %record_ptr255 = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_longitude, ptr null, i32 1) to i64))
  %field_0256 = getelementptr %struct_longitude, ptr %record_ptr255, i32 0, i32 0
  store double 1.000000e+02, ptr %field_0256, align 8
  %39 = getelementptr ptr, ptr %__current_row_load254, i64 0
  %loaded_field257 = load ptr, ptr %39, align 8
  %40 = getelementptr ptr, ptr %__current_row_load254, i64 1
  %loaded_field258 = load ptr, ptr %40, align 8
  %41 = getelementptr ptr, ptr %record_ptr255, i64 0
  %loaded_field259 = load ptr, ptr %41, align 8
  %42 = getelementptr ptr, ptr %__current_row_load254, i64 3
  %loaded_field260 = load ptr, ptr %42, align 8
  %43 = getelementptr ptr, ptr %__current_row_load254, i64 4
  %loaded_field261 = load ptr, ptr %43, align 8
  %44 = getelementptr ptr, ptr %__current_row_load254, i64 5
  %loaded_field262 = load ptr, ptr %44, align 8
  %45 = getelementptr ptr, ptr %__current_row_load254, i64 6
  %loaded_field263 = load ptr, ptr %45, align 8
  %46 = getelementptr ptr, ptr %__current_row_load254, i64 7
  %loaded_field264 = load ptr, ptr %46, align 8
  %47 = getelementptr ptr, ptr %__current_row_load254, i64 8
  %loaded_field265 = load ptr, ptr %47, align 8
  %48 = getelementptr ptr, ptr %__current_row_load254, i64 9
  %loaded_field266 = load ptr, ptr %48, align 8
  %49 = getelementptr ptr, ptr %__current_row_load254, i64 10
  %loaded_field267 = load ptr, ptr %49, align 8
  %50 = getelementptr ptr, ptr %__current_row_load254, i64 11
  %loaded_field268 = load ptr, ptr %50, align 8
  %51 = getelementptr ptr, ptr %__current_row_load254, i64 12
  %loaded_field269 = load ptr, ptr %51, align 8
  %52 = getelementptr ptr, ptr %__current_row_load254, i64 13
  %loaded_field270 = load ptr, ptr %52, align 8
  %53 = getelementptr ptr, ptr %__current_row_load254, i64 14
  %loaded_field271 = load ptr, ptr %53, align 8
  %54 = getelementptr ptr, ptr %__current_row_load254, i64 15
  %loaded_field272 = load ptr, ptr %54, align 8
  %55 = getelementptr ptr, ptr %__current_row_load254, i64 16
  %loaded_field273 = load ptr, ptr %55, align 8
  %56 = getelementptr ptr, ptr %__current_row_load254, i64 17
  %loaded_field274 = load ptr, ptr %56, align 8
  %57 = getelementptr ptr, ptr %__current_row_load254, i64 18
  %loaded_field275 = load ptr, ptr %57, align 8
  %58 = getelementptr ptr, ptr %__current_row_load254, i64 19
  %loaded_field276 = load ptr, ptr %58, align 8
  %59 = getelementptr ptr, ptr %__current_row_load254, i64 20
  %loaded_field277 = load ptr, ptr %59, align 8
  %60 = getelementptr ptr, ptr %__current_row_load254, i64 21
  %loaded_field278 = load ptr, ptr %60, align 8
  %61 = getelementptr ptr, ptr %__current_row_load254, i64 22
  %loaded_field279 = load ptr, ptr %61, align 8
  %62 = getelementptr ptr, ptr %__current_row_load254, i64 23
  %loaded_field280 = load ptr, ptr %62, align 8
  %63 = getelementptr ptr, ptr %__current_row_load254, i64 24
  %loaded_field281 = load ptr, ptr %63, align 8
  %64 = getelementptr ptr, ptr %__current_row_load254, i64 25
  %loaded_field282 = load ptr, ptr %64, align 8
  %65 = getelementptr ptr, ptr %__current_row_load254, i64 26
  %loaded_field283 = load ptr, ptr %65, align 8
  %66 = getelementptr ptr, ptr %__current_row_load254, i64 27
  %loaded_field284 = load ptr, ptr %66, align 8
  %67 = getelementptr ptr, ptr %__current_row_load254, i64 28
  %loaded_field285 = load ptr, ptr %67, align 8
  %68 = getelementptr ptr, ptr %__current_row_load254, i64 29
  %loaded_field286 = load ptr, ptr %68, align 8
  %69 = getelementptr ptr, ptr %__current_row_load254, i64 30
  %loaded_field287 = load ptr, ptr %69, align 8
  %70 = getelementptr ptr, ptr %__current_row_load254, i64 31
  %loaded_field288 = load ptr, ptr %70, align 8
  %71 = getelementptr ptr, ptr %__current_row_load254, i64 32
  %loaded_field289 = load ptr, ptr %71, align 8
  %72 = getelementptr ptr, ptr %__current_row_load254, i64 33
  %loaded_field290 = load ptr, ptr %72, align 8
  %73 = getelementptr ptr, ptr %__current_row_load254, i64 34
  %loaded_field291 = load ptr, ptr %73, align 8
  %74 = getelementptr ptr, ptr %__current_row_load254, i64 35
  %loaded_field292 = load ptr, ptr %74, align 8
  %75 = getelementptr ptr, ptr %__current_row_load254, i64 36
  %loaded_field293 = load ptr, ptr %75, align 8
  %rec_ptr294 = call ptr @malloc(i64 296)
  %slot_0295 = getelementptr ptr, ptr %rec_ptr294, i64 0
  store ptr %loaded_field257, ptr %slot_0295, align 8
  %slot_1296 = getelementptr ptr, ptr %rec_ptr294, i64 1
  store ptr %loaded_field258, ptr %slot_1296, align 8
  %slot_2297 = getelementptr ptr, ptr %rec_ptr294, i64 2
  store ptr %loaded_field259, ptr %slot_2297, align 8
  %slot_3298 = getelementptr ptr, ptr %rec_ptr294, i64 3
  store ptr %loaded_field260, ptr %slot_3298, align 8
  %slot_4299 = getelementptr ptr, ptr %rec_ptr294, i64 4
  store ptr %loaded_field261, ptr %slot_4299, align 8
  %slot_5300 = getelementptr ptr, ptr %rec_ptr294, i64 5
  store ptr %loaded_field262, ptr %slot_5300, align 8
  %slot_6301 = getelementptr ptr, ptr %rec_ptr294, i64 6
  store ptr %loaded_field263, ptr %slot_6301, align 8
  %slot_7302 = getelementptr ptr, ptr %rec_ptr294, i64 7
  store ptr %loaded_field264, ptr %slot_7302, align 8
  %slot_8303 = getelementptr ptr, ptr %rec_ptr294, i64 8
  store ptr %loaded_field265, ptr %slot_8303, align 8
  %slot_9304 = getelementptr ptr, ptr %rec_ptr294, i64 9
  store ptr %loaded_field266, ptr %slot_9304, align 8
  %slot_10305 = getelementptr ptr, ptr %rec_ptr294, i64 10
  store ptr %loaded_field267, ptr %slot_10305, align 8
  %slot_11306 = getelementptr ptr, ptr %rec_ptr294, i64 11
  store ptr %loaded_field268, ptr %slot_11306, align 8
  %slot_12307 = getelementptr ptr, ptr %rec_ptr294, i64 12
  store ptr %loaded_field269, ptr %slot_12307, align 8
  %slot_13308 = getelementptr ptr, ptr %rec_ptr294, i64 13
  store ptr %loaded_field270, ptr %slot_13308, align 8
  %slot_14309 = getelementptr ptr, ptr %rec_ptr294, i64 14
  store ptr %loaded_field271, ptr %slot_14309, align 8
  %slot_15310 = getelementptr ptr, ptr %rec_ptr294, i64 15
  store ptr %loaded_field272, ptr %slot_15310, align 8
  %slot_16311 = getelementptr ptr, ptr %rec_ptr294, i64 16
  store ptr %loaded_field273, ptr %slot_16311, align 8
  %slot_17312 = getelementptr ptr, ptr %rec_ptr294, i64 17
  store ptr %loaded_field274, ptr %slot_17312, align 8
  %slot_18313 = getelementptr ptr, ptr %rec_ptr294, i64 18
  store ptr %loaded_field275, ptr %slot_18313, align 8
  %slot_19314 = getelementptr ptr, ptr %rec_ptr294, i64 19
  store ptr %loaded_field276, ptr %slot_19314, align 8
  %slot_20315 = getelementptr ptr, ptr %rec_ptr294, i64 20
  store ptr %loaded_field277, ptr %slot_20315, align 8
  %slot_21316 = getelementptr ptr, ptr %rec_ptr294, i64 21
  store ptr %loaded_field278, ptr %slot_21316, align 8
  %slot_22317 = getelementptr ptr, ptr %rec_ptr294, i64 22
  store ptr %loaded_field279, ptr %slot_22317, align 8
  %slot_23318 = getelementptr ptr, ptr %rec_ptr294, i64 23
  store ptr %loaded_field280, ptr %slot_23318, align 8
  %slot_24319 = getelementptr ptr, ptr %rec_ptr294, i64 24
  store ptr %loaded_field281, ptr %slot_24319, align 8
  %slot_25320 = getelementptr ptr, ptr %rec_ptr294, i64 25
  store ptr %loaded_field282, ptr %slot_25320, align 8
  %slot_26321 = getelementptr ptr, ptr %rec_ptr294, i64 26
  store ptr %loaded_field283, ptr %slot_26321, align 8
  %slot_27322 = getelementptr ptr, ptr %rec_ptr294, i64 27
  store ptr %loaded_field284, ptr %slot_27322, align 8
  %slot_28323 = getelementptr ptr, ptr %rec_ptr294, i64 28
  store ptr %loaded_field285, ptr %slot_28323, align 8
  %slot_29324 = getelementptr ptr, ptr %rec_ptr294, i64 29
  store ptr %loaded_field286, ptr %slot_29324, align 8
  %slot_30325 = getelementptr ptr, ptr %rec_ptr294, i64 30
  store ptr %loaded_field287, ptr %slot_30325, align 8
  %slot_31326 = getelementptr ptr, ptr %rec_ptr294, i64 31
  store ptr %loaded_field288, ptr %slot_31326, align 8
  %slot_32327 = getelementptr ptr, ptr %rec_ptr294, i64 32
  store ptr %loaded_field289, ptr %slot_32327, align 8
  %slot_33328 = getelementptr ptr, ptr %rec_ptr294, i64 33
  store ptr %loaded_field290, ptr %slot_33328, align 8
  %slot_34329 = getelementptr ptr, ptr %rec_ptr294, i64 34
  store ptr %loaded_field291, ptr %slot_34329, align 8
  %slot_35330 = getelementptr ptr, ptr %rec_ptr294, i64 35
  store ptr %loaded_field292, ptr %slot_35330, align 8
  %slot_36331 = getelementptr ptr, ptr %rec_ptr294, i64 36
  store ptr %loaded_field293, ptr %slot_36331, align 8
  %rows_field332 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_result_load250, i32 0, i32 1
  %rows_array333 = load ptr, ptr %rows_field332, align 8
  %len_ptr334 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array333, i32 0, i32 0
  %cap_ptr335 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array333, i32 0, i32 1
  %data_ptr_ptr336 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array333, i32 0, i32 2
  %len337 = load i64, ptr %len_ptr334, align 8
  %cap338 = load i64, ptr %cap_ptr335, align 8
  %data339 = load ptr, ptr %data_ptr_ptr336, align 8
  %is_full340 = icmp uge i64 %len337, %cap338
  br i1 %is_full340, label %grow341, label %cont342

for.step231:                                      ; preds = %cont342
  %x_load349 = load i64, ptr @__map_i, align 8
  %inc_add350 = add i64 %x_load349, 1
  store i64 %inc_add350, ptr @__map_i, align 8
  br label %for.cond229, !llvm.loop !0

for.end232:                                       ; preds = %for.cond229
  %__map_result_load351 = load ptr, ptr @__map_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %__map_result_load351, ptr %data_ptr, align 8
  ret ptr %runtime_obj

grow341:                                          ; preds = %for.body230
  %76 = icmp eq i64 %cap338, 0
  %77 = mul i64 %cap338, 2
  %new_cap343 = select i1 %76, i64 4, i64 %77
  %bytes344 = mul i64 %new_cap343, 8
  %realloc345 = call ptr @realloc(ptr %data339, i64 %bytes344)
  store i64 %new_cap343, ptr %cap_ptr335, align 8
  store ptr %realloc345, ptr %data_ptr_ptr336, align 8
  br label %cont342

cont342:                                          ; preds = %grow341, %for.body230
  %final_data_ptr346 = phi ptr [ %data339, %for.body230 ], [ %realloc345, %grow341 ]
  %target_slot_ptr347 = getelementptr ptr, ptr %final_data_ptr346, i64 %len337
  store ptr %rec_ptr294, ptr %target_slot_ptr347, align 8
  %new_len348 = add i64 %len337, 1
  store i64 %new_len348, ptr %len_ptr334, align 8
  br label %for.step231
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i8 1}
