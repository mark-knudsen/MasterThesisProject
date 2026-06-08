; ModuleID = 'repl_module'
source_filename = "repl_module"

%struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType = type { ptr, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, i64, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8 }
%dataframe = type { ptr, ptr, ptr, i64 }
%struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17 = type { ptr, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, i64, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8, i8 }

@df = external global ptr
@__where_src_0 = global ptr null
@__where_mask_0 = global ptr null
@__where_i_0 = global i64 0
@err_msg = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.1 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.2 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.3 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.4 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.5 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.6 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.7 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.8 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.9 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.10 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.11 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.12 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.13 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.14 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.15 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.16 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.17 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.18 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.19 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.20 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.21 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.22 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.23 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.24 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.25 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.26 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.27 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.28 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.29 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.30 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.31 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.32 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.33 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.34 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.35 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.36 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@x = global ptr null
@err_msg.37 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@__mask_count_0 = global i64 0
@err_msg.38 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@str = private unnamed_addr constant [5 x i8] c"date\00", align 1
@str.39 = private unnamed_addr constant [9 x i8] c"latitude\00", align 1
@str.40 = private unnamed_addr constant [10 x i8] c"longitude\00", align 1
@str.41 = private unnamed_addr constant [15 x i8] c"wind-speed-min\00", align 1
@str.42 = private unnamed_addr constant [15 x i8] c"wind-speed-max\00", align 1
@str.43 = private unnamed_addr constant [16 x i8] c"wind-speed-mean\00", align 1
@str.44 = private unnamed_addr constant [19 x i8] c"wind-direction-min\00", align 1
@str.45 = private unnamed_addr constant [19 x i8] c"wind-direction-max\00", align 1
@str.46 = private unnamed_addr constant [20 x i8] c"wind-direction-mean\00", align 1
@str.47 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-min\00", align 1
@str.48 = private unnamed_addr constant [28 x i8] c"surface-air-temperature-max\00", align 1
@str.49 = private unnamed_addr constant [29 x i8] c"surface-air-temperature-mean\00", align 1
@str.50 = private unnamed_addr constant [19 x i8] c"total-rainfall-sum\00", align 1
@str.51 = private unnamed_addr constant [21 x i8] c"surface-humidity-min\00", align 1
@str.52 = private unnamed_addr constant [21 x i8] c"surface-humidity-max\00", align 1
@str.53 = private unnamed_addr constant [22 x i8] c"surface-humidity-mean\00", align 1
@str.54 = private unnamed_addr constant [5 x i8] c"ndvi\00", align 1
@str.55 = private unnamed_addr constant [10 x i8] c"elevation\00", align 1
@str.56 = private unnamed_addr constant [6 x i8] c"slope\00", align 1
@str.57 = private unnamed_addr constant [7 x i8] c"aspect\00", align 1
@str.58 = private unnamed_addr constant [11 x i8] c"fire_label\00", align 1
@str.59 = private unnamed_addr constant [19 x i8] c"land_cover_class_1\00", align 1
@str.60 = private unnamed_addr constant [19 x i8] c"land_cover_class_2\00", align 1
@str.61 = private unnamed_addr constant [19 x i8] c"land_cover_class_4\00", align 1
@str.62 = private unnamed_addr constant [19 x i8] c"land_cover_class_5\00", align 1
@str.63 = private unnamed_addr constant [19 x i8] c"land_cover_class_6\00", align 1
@str.64 = private unnamed_addr constant [19 x i8] c"land_cover_class_7\00", align 1
@str.65 = private unnamed_addr constant [19 x i8] c"land_cover_class_8\00", align 1
@str.66 = private unnamed_addr constant [19 x i8] c"land_cover_class_9\00", align 1
@str.67 = private unnamed_addr constant [20 x i8] c"land_cover_class_10\00", align 1
@str.68 = private unnamed_addr constant [20 x i8] c"land_cover_class_11\00", align 1
@str.69 = private unnamed_addr constant [20 x i8] c"land_cover_class_12\00", align 1
@str.70 = private unnamed_addr constant [20 x i8] c"land_cover_class_13\00", align 1
@str.71 = private unnamed_addr constant [20 x i8] c"land_cover_class_14\00", align 1
@str.72 = private unnamed_addr constant [20 x i8] c"land_cover_class_15\00", align 1
@str.73 = private unnamed_addr constant [20 x i8] c"land_cover_class_16\00", align 1
@str.74 = private unnamed_addr constant [20 x i8] c"land_cover_class_17\00", align 1
@__where_result_0 = global ptr null
@__where_res_i_0 = global i64 0
@err_msg.75 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.76 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.77 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.78 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.79 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.80 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.81 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.82 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.83 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.84 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.85 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.86 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.87 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.88 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.89 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.90 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.91 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.92 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.93 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.94 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.95 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.96 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.97 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.98 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.99 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.100 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.101 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.102 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.103 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.104 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.105 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.106 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.107 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.108 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.109 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.110 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.111 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.112 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.113 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.114 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.115 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.116 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.117 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.118 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.119 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.120 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.121 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.122 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.123 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.124 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.125 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.126 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.127 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.128 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.129 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.130 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.131 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.132 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.133 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.134 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.135 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.136 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.137 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.138 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.139 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.140 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.141 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.142 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.143 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.144 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.145 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.146 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.147 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.148 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1
@err_msg.149 = private unnamed_addr constant [37 x i8] c"Runtime Error: Index Out of Bounds!\0A\00", align 1

define ptr @main_2() {
entry:
  %df_load = load ptr, ptr @df, align 8
  store ptr %df_load, ptr @__where_src_0, align 8
  %arr_header = call ptr @malloc(i64 24)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %0, align 8
  %arr_data = call ptr @malloc(i64 100)
  store i64 100, ptr %1, align 8
  store ptr %arr_data, ptr %2, align 8
  store ptr %arr_header, ptr @__where_mask_0, align 8
  %__where_mask_0_load = load ptr, ptr @__where_mask_0, align 8
  %__where_src_0_load = load ptr, ptr @__where_src_0, align 8
  %rowCount_ptr = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %__where_src_0_load, i32 0, i32 3
  %rowCount = load i64, ptr %rowCount_ptr, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_mask_0_load, i32 0, i32 0
  store i64 %rowCount, ptr %len_ptr, align 8
  store i64 0, ptr @__where_i_0, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__where_i_0_load = load i64, ptr @__where_i_0, align 8
  %__where_src_0_load1 = load ptr, ptr @__where_src_0, align 8
  %rowCount_ptr2 = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %__where_src_0_load1, i32 0, i32 3
  %rowCount3 = load i64, ptr %rowCount_ptr2, align 8
  %icmp_tmp = icmp slt i64 %__where_i_0_load, %rowCount3
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %record_mem = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr null, i32 1) to i32))
  %__where_src_0_load4 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load4, i32 0, i32 1
  %data_header = load ptr, ptr %df_data_ptr, align 8
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 2
  %4 = load ptr, ptr %3, align 8
  %5 = getelementptr ptr, ptr %4, i64 0
  %6 = load ptr, ptr %5, align 8
  %__where_i_0_load5 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %6, i32 0, i32 0
  %data_field_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %6, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_field_ptr, align 8
  %array_len = load i64, ptr %len_field_ptr, align 8
  %index_is_neg = icmp slt i64 %__where_i_0_load5, 0
  %index_rel = add i64 %__where_i_0_load5, %array_len
  %resolved_index = select i1 %index_is_neg, i64 %index_rel, i64 %__where_i_0_load5
  %is_neg = icmp slt i64 %resolved_index, 0
  %is_too_big = icmp sge i64 %resolved_index, %array_len
  %is_invalid = or i1 %is_neg, %is_too_big
  br i1 %is_invalid, label %bounds.fail, label %bounds.ok

for.step:                                         ; preds = %assign_bounds.ok
  %x_load699 = load i64, ptr @__where_i_0, align 8
  %inc_add = add i64 %x_load699, 1
  store i64 %inc_add, ptr @__where_i_0, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  store i64 0, ptr @__mask_count_0, align 8
  store i64 0, ptr @__where_i_0, align 8
  br label %for.cond700

bounds.fail:                                      ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @err_msg)
  ret ptr null

bounds.ok:                                        ; preds = %for.body
  %elem_ptr = getelementptr ptr, ptr %data_ptr, i64 %resolved_index
  %loaded_val = load ptr, ptr %elem_ptr, align 8
  %ptr_date = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 0
  store ptr %loaded_val, ptr %ptr_date, align 8
  %__where_src_0_load6 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr7 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load6, i32 0, i32 1
  %data_header8 = load ptr, ptr %df_data_ptr7, align 8
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header8, i32 0, i32 2
  %8 = load ptr, ptr %7, align 8
  %9 = getelementptr ptr, ptr %8, i64 1
  %10 = load ptr, ptr %9, align 8
  %__where_i_0_load9 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %10, i32 0, i32 0
  %data_field_ptr11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %10, i32 0, i32 2
  %data_ptr12 = load ptr, ptr %data_field_ptr11, align 8
  %array_len13 = load i64, ptr %len_field_ptr10, align 8
  %index_is_neg14 = icmp slt i64 %__where_i_0_load9, 0
  %index_rel15 = add i64 %__where_i_0_load9, %array_len13
  %resolved_index16 = select i1 %index_is_neg14, i64 %index_rel15, i64 %__where_i_0_load9
  %is_neg17 = icmp slt i64 %resolved_index16, 0
  %is_too_big18 = icmp sge i64 %resolved_index16, %array_len13
  %is_invalid19 = or i1 %is_neg17, %is_too_big18
  br i1 %is_invalid19, label %bounds.fail20, label %bounds.ok21

bounds.fail20:                                    ; preds = %bounds.ok
  %print_err22 = call i32 (ptr, ...) @printf(ptr @err_msg.1)
  ret ptr null

bounds.ok21:                                      ; preds = %bounds.ok
  %elem_ptr23 = getelementptr double, ptr %data_ptr12, i64 %resolved_index16
  %loaded_val24 = load double, ptr %elem_ptr23, align 8
  %ptr_latitude = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 1
  store double %loaded_val24, ptr %ptr_latitude, align 8
  %__where_src_0_load25 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr26 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load25, i32 0, i32 1
  %data_header27 = load ptr, ptr %df_data_ptr26, align 8
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header27, i32 0, i32 2
  %12 = load ptr, ptr %11, align 8
  %13 = getelementptr ptr, ptr %12, i64 2
  %14 = load ptr, ptr %13, align 8
  %__where_i_0_load28 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr29 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %14, i32 0, i32 0
  %data_field_ptr30 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %14, i32 0, i32 2
  %data_ptr31 = load ptr, ptr %data_field_ptr30, align 8
  %array_len32 = load i64, ptr %len_field_ptr29, align 8
  %index_is_neg33 = icmp slt i64 %__where_i_0_load28, 0
  %index_rel34 = add i64 %__where_i_0_load28, %array_len32
  %resolved_index35 = select i1 %index_is_neg33, i64 %index_rel34, i64 %__where_i_0_load28
  %is_neg36 = icmp slt i64 %resolved_index35, 0
  %is_too_big37 = icmp sge i64 %resolved_index35, %array_len32
  %is_invalid38 = or i1 %is_neg36, %is_too_big37
  br i1 %is_invalid38, label %bounds.fail39, label %bounds.ok40

bounds.fail39:                                    ; preds = %bounds.ok21
  %print_err41 = call i32 (ptr, ...) @printf(ptr @err_msg.2)
  ret ptr null

bounds.ok40:                                      ; preds = %bounds.ok21
  %elem_ptr42 = getelementptr double, ptr %data_ptr31, i64 %resolved_index35
  %loaded_val43 = load double, ptr %elem_ptr42, align 8
  %ptr_longitude = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 2
  store double %loaded_val43, ptr %ptr_longitude, align 8
  %__where_src_0_load44 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr45 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load44, i32 0, i32 1
  %data_header46 = load ptr, ptr %df_data_ptr45, align 8
  %15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header46, i32 0, i32 2
  %16 = load ptr, ptr %15, align 8
  %17 = getelementptr ptr, ptr %16, i64 3
  %18 = load ptr, ptr %17, align 8
  %__where_i_0_load47 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr48 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %18, i32 0, i32 0
  %data_field_ptr49 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %18, i32 0, i32 2
  %data_ptr50 = load ptr, ptr %data_field_ptr49, align 8
  %array_len51 = load i64, ptr %len_field_ptr48, align 8
  %index_is_neg52 = icmp slt i64 %__where_i_0_load47, 0
  %index_rel53 = add i64 %__where_i_0_load47, %array_len51
  %resolved_index54 = select i1 %index_is_neg52, i64 %index_rel53, i64 %__where_i_0_load47
  %is_neg55 = icmp slt i64 %resolved_index54, 0
  %is_too_big56 = icmp sge i64 %resolved_index54, %array_len51
  %is_invalid57 = or i1 %is_neg55, %is_too_big56
  br i1 %is_invalid57, label %bounds.fail58, label %bounds.ok59

bounds.fail58:                                    ; preds = %bounds.ok40
  %print_err60 = call i32 (ptr, ...) @printf(ptr @err_msg.3)
  ret ptr null

bounds.ok59:                                      ; preds = %bounds.ok40
  %elem_ptr61 = getelementptr double, ptr %data_ptr50, i64 %resolved_index54
  %loaded_val62 = load double, ptr %elem_ptr61, align 8
  %ptr_wind-speed-min = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 3
  store double %loaded_val62, ptr %ptr_wind-speed-min, align 8
  %__where_src_0_load63 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr64 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load63, i32 0, i32 1
  %data_header65 = load ptr, ptr %df_data_ptr64, align 8
  %19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header65, i32 0, i32 2
  %20 = load ptr, ptr %19, align 8
  %21 = getelementptr ptr, ptr %20, i64 4
  %22 = load ptr, ptr %21, align 8
  %__where_i_0_load66 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr67 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %22, i32 0, i32 0
  %data_field_ptr68 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %22, i32 0, i32 2
  %data_ptr69 = load ptr, ptr %data_field_ptr68, align 8
  %array_len70 = load i64, ptr %len_field_ptr67, align 8
  %index_is_neg71 = icmp slt i64 %__where_i_0_load66, 0
  %index_rel72 = add i64 %__where_i_0_load66, %array_len70
  %resolved_index73 = select i1 %index_is_neg71, i64 %index_rel72, i64 %__where_i_0_load66
  %is_neg74 = icmp slt i64 %resolved_index73, 0
  %is_too_big75 = icmp sge i64 %resolved_index73, %array_len70
  %is_invalid76 = or i1 %is_neg74, %is_too_big75
  br i1 %is_invalid76, label %bounds.fail77, label %bounds.ok78

bounds.fail77:                                    ; preds = %bounds.ok59
  %print_err79 = call i32 (ptr, ...) @printf(ptr @err_msg.4)
  ret ptr null

bounds.ok78:                                      ; preds = %bounds.ok59
  %elem_ptr80 = getelementptr double, ptr %data_ptr69, i64 %resolved_index73
  %loaded_val81 = load double, ptr %elem_ptr80, align 8
  %ptr_wind-speed-max = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 4
  store double %loaded_val81, ptr %ptr_wind-speed-max, align 8
  %__where_src_0_load82 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr83 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load82, i32 0, i32 1
  %data_header84 = load ptr, ptr %df_data_ptr83, align 8
  %23 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header84, i32 0, i32 2
  %24 = load ptr, ptr %23, align 8
  %25 = getelementptr ptr, ptr %24, i64 5
  %26 = load ptr, ptr %25, align 8
  %__where_i_0_load85 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr86 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %26, i32 0, i32 0
  %data_field_ptr87 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %26, i32 0, i32 2
  %data_ptr88 = load ptr, ptr %data_field_ptr87, align 8
  %array_len89 = load i64, ptr %len_field_ptr86, align 8
  %index_is_neg90 = icmp slt i64 %__where_i_0_load85, 0
  %index_rel91 = add i64 %__where_i_0_load85, %array_len89
  %resolved_index92 = select i1 %index_is_neg90, i64 %index_rel91, i64 %__where_i_0_load85
  %is_neg93 = icmp slt i64 %resolved_index92, 0
  %is_too_big94 = icmp sge i64 %resolved_index92, %array_len89
  %is_invalid95 = or i1 %is_neg93, %is_too_big94
  br i1 %is_invalid95, label %bounds.fail96, label %bounds.ok97

bounds.fail96:                                    ; preds = %bounds.ok78
  %print_err98 = call i32 (ptr, ...) @printf(ptr @err_msg.5)
  ret ptr null

bounds.ok97:                                      ; preds = %bounds.ok78
  %elem_ptr99 = getelementptr double, ptr %data_ptr88, i64 %resolved_index92
  %loaded_val100 = load double, ptr %elem_ptr99, align 8
  %ptr_wind-speed-mean = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 5
  store double %loaded_val100, ptr %ptr_wind-speed-mean, align 8
  %__where_src_0_load101 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr102 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load101, i32 0, i32 1
  %data_header103 = load ptr, ptr %df_data_ptr102, align 8
  %27 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header103, i32 0, i32 2
  %28 = load ptr, ptr %27, align 8
  %29 = getelementptr ptr, ptr %28, i64 6
  %30 = load ptr, ptr %29, align 8
  %__where_i_0_load104 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr105 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %30, i32 0, i32 0
  %data_field_ptr106 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %30, i32 0, i32 2
  %data_ptr107 = load ptr, ptr %data_field_ptr106, align 8
  %array_len108 = load i64, ptr %len_field_ptr105, align 8
  %index_is_neg109 = icmp slt i64 %__where_i_0_load104, 0
  %index_rel110 = add i64 %__where_i_0_load104, %array_len108
  %resolved_index111 = select i1 %index_is_neg109, i64 %index_rel110, i64 %__where_i_0_load104
  %is_neg112 = icmp slt i64 %resolved_index111, 0
  %is_too_big113 = icmp sge i64 %resolved_index111, %array_len108
  %is_invalid114 = or i1 %is_neg112, %is_too_big113
  br i1 %is_invalid114, label %bounds.fail115, label %bounds.ok116

bounds.fail115:                                   ; preds = %bounds.ok97
  %print_err117 = call i32 (ptr, ...) @printf(ptr @err_msg.6)
  ret ptr null

bounds.ok116:                                     ; preds = %bounds.ok97
  %elem_ptr118 = getelementptr double, ptr %data_ptr107, i64 %resolved_index111
  %loaded_val119 = load double, ptr %elem_ptr118, align 8
  %ptr_wind-direction-min = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 6
  store double %loaded_val119, ptr %ptr_wind-direction-min, align 8
  %__where_src_0_load120 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr121 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load120, i32 0, i32 1
  %data_header122 = load ptr, ptr %df_data_ptr121, align 8
  %31 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header122, i32 0, i32 2
  %32 = load ptr, ptr %31, align 8
  %33 = getelementptr ptr, ptr %32, i64 7
  %34 = load ptr, ptr %33, align 8
  %__where_i_0_load123 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr124 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %34, i32 0, i32 0
  %data_field_ptr125 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %34, i32 0, i32 2
  %data_ptr126 = load ptr, ptr %data_field_ptr125, align 8
  %array_len127 = load i64, ptr %len_field_ptr124, align 8
  %index_is_neg128 = icmp slt i64 %__where_i_0_load123, 0
  %index_rel129 = add i64 %__where_i_0_load123, %array_len127
  %resolved_index130 = select i1 %index_is_neg128, i64 %index_rel129, i64 %__where_i_0_load123
  %is_neg131 = icmp slt i64 %resolved_index130, 0
  %is_too_big132 = icmp sge i64 %resolved_index130, %array_len127
  %is_invalid133 = or i1 %is_neg131, %is_too_big132
  br i1 %is_invalid133, label %bounds.fail134, label %bounds.ok135

bounds.fail134:                                   ; preds = %bounds.ok116
  %print_err136 = call i32 (ptr, ...) @printf(ptr @err_msg.7)
  ret ptr null

bounds.ok135:                                     ; preds = %bounds.ok116
  %elem_ptr137 = getelementptr double, ptr %data_ptr126, i64 %resolved_index130
  %loaded_val138 = load double, ptr %elem_ptr137, align 8
  %ptr_wind-direction-max = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 7
  store double %loaded_val138, ptr %ptr_wind-direction-max, align 8
  %__where_src_0_load139 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr140 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load139, i32 0, i32 1
  %data_header141 = load ptr, ptr %df_data_ptr140, align 8
  %35 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header141, i32 0, i32 2
  %36 = load ptr, ptr %35, align 8
  %37 = getelementptr ptr, ptr %36, i64 8
  %38 = load ptr, ptr %37, align 8
  %__where_i_0_load142 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr143 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %38, i32 0, i32 0
  %data_field_ptr144 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %38, i32 0, i32 2
  %data_ptr145 = load ptr, ptr %data_field_ptr144, align 8
  %array_len146 = load i64, ptr %len_field_ptr143, align 8
  %index_is_neg147 = icmp slt i64 %__where_i_0_load142, 0
  %index_rel148 = add i64 %__where_i_0_load142, %array_len146
  %resolved_index149 = select i1 %index_is_neg147, i64 %index_rel148, i64 %__where_i_0_load142
  %is_neg150 = icmp slt i64 %resolved_index149, 0
  %is_too_big151 = icmp sge i64 %resolved_index149, %array_len146
  %is_invalid152 = or i1 %is_neg150, %is_too_big151
  br i1 %is_invalid152, label %bounds.fail153, label %bounds.ok154

bounds.fail153:                                   ; preds = %bounds.ok135
  %print_err155 = call i32 (ptr, ...) @printf(ptr @err_msg.8)
  ret ptr null

bounds.ok154:                                     ; preds = %bounds.ok135
  %elem_ptr156 = getelementptr double, ptr %data_ptr145, i64 %resolved_index149
  %loaded_val157 = load double, ptr %elem_ptr156, align 8
  %ptr_wind-direction-mean = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 8
  store double %loaded_val157, ptr %ptr_wind-direction-mean, align 8
  %__where_src_0_load158 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr159 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load158, i32 0, i32 1
  %data_header160 = load ptr, ptr %df_data_ptr159, align 8
  %39 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header160, i32 0, i32 2
  %40 = load ptr, ptr %39, align 8
  %41 = getelementptr ptr, ptr %40, i64 9
  %42 = load ptr, ptr %41, align 8
  %__where_i_0_load161 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr162 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %42, i32 0, i32 0
  %data_field_ptr163 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %42, i32 0, i32 2
  %data_ptr164 = load ptr, ptr %data_field_ptr163, align 8
  %array_len165 = load i64, ptr %len_field_ptr162, align 8
  %index_is_neg166 = icmp slt i64 %__where_i_0_load161, 0
  %index_rel167 = add i64 %__where_i_0_load161, %array_len165
  %resolved_index168 = select i1 %index_is_neg166, i64 %index_rel167, i64 %__where_i_0_load161
  %is_neg169 = icmp slt i64 %resolved_index168, 0
  %is_too_big170 = icmp sge i64 %resolved_index168, %array_len165
  %is_invalid171 = or i1 %is_neg169, %is_too_big170
  br i1 %is_invalid171, label %bounds.fail172, label %bounds.ok173

bounds.fail172:                                   ; preds = %bounds.ok154
  %print_err174 = call i32 (ptr, ...) @printf(ptr @err_msg.9)
  ret ptr null

bounds.ok173:                                     ; preds = %bounds.ok154
  %elem_ptr175 = getelementptr double, ptr %data_ptr164, i64 %resolved_index168
  %loaded_val176 = load double, ptr %elem_ptr175, align 8
  %ptr_surface-air-temperature-min = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 9
  store double %loaded_val176, ptr %ptr_surface-air-temperature-min, align 8
  %__where_src_0_load177 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr178 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load177, i32 0, i32 1
  %data_header179 = load ptr, ptr %df_data_ptr178, align 8
  %43 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header179, i32 0, i32 2
  %44 = load ptr, ptr %43, align 8
  %45 = getelementptr ptr, ptr %44, i64 10
  %46 = load ptr, ptr %45, align 8
  %__where_i_0_load180 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr181 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %46, i32 0, i32 0
  %data_field_ptr182 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %46, i32 0, i32 2
  %data_ptr183 = load ptr, ptr %data_field_ptr182, align 8
  %array_len184 = load i64, ptr %len_field_ptr181, align 8
  %index_is_neg185 = icmp slt i64 %__where_i_0_load180, 0
  %index_rel186 = add i64 %__where_i_0_load180, %array_len184
  %resolved_index187 = select i1 %index_is_neg185, i64 %index_rel186, i64 %__where_i_0_load180
  %is_neg188 = icmp slt i64 %resolved_index187, 0
  %is_too_big189 = icmp sge i64 %resolved_index187, %array_len184
  %is_invalid190 = or i1 %is_neg188, %is_too_big189
  br i1 %is_invalid190, label %bounds.fail191, label %bounds.ok192

bounds.fail191:                                   ; preds = %bounds.ok173
  %print_err193 = call i32 (ptr, ...) @printf(ptr @err_msg.10)
  ret ptr null

bounds.ok192:                                     ; preds = %bounds.ok173
  %elem_ptr194 = getelementptr double, ptr %data_ptr183, i64 %resolved_index187
  %loaded_val195 = load double, ptr %elem_ptr194, align 8
  %ptr_surface-air-temperature-max = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 10
  store double %loaded_val195, ptr %ptr_surface-air-temperature-max, align 8
  %__where_src_0_load196 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr197 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load196, i32 0, i32 1
  %data_header198 = load ptr, ptr %df_data_ptr197, align 8
  %47 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header198, i32 0, i32 2
  %48 = load ptr, ptr %47, align 8
  %49 = getelementptr ptr, ptr %48, i64 11
  %50 = load ptr, ptr %49, align 8
  %__where_i_0_load199 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr200 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %50, i32 0, i32 0
  %data_field_ptr201 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %50, i32 0, i32 2
  %data_ptr202 = load ptr, ptr %data_field_ptr201, align 8
  %array_len203 = load i64, ptr %len_field_ptr200, align 8
  %index_is_neg204 = icmp slt i64 %__where_i_0_load199, 0
  %index_rel205 = add i64 %__where_i_0_load199, %array_len203
  %resolved_index206 = select i1 %index_is_neg204, i64 %index_rel205, i64 %__where_i_0_load199
  %is_neg207 = icmp slt i64 %resolved_index206, 0
  %is_too_big208 = icmp sge i64 %resolved_index206, %array_len203
  %is_invalid209 = or i1 %is_neg207, %is_too_big208
  br i1 %is_invalid209, label %bounds.fail210, label %bounds.ok211

bounds.fail210:                                   ; preds = %bounds.ok192
  %print_err212 = call i32 (ptr, ...) @printf(ptr @err_msg.11)
  ret ptr null

bounds.ok211:                                     ; preds = %bounds.ok192
  %elem_ptr213 = getelementptr double, ptr %data_ptr202, i64 %resolved_index206
  %loaded_val214 = load double, ptr %elem_ptr213, align 8
  %ptr_surface-air-temperature-mean = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 11
  store double %loaded_val214, ptr %ptr_surface-air-temperature-mean, align 8
  %__where_src_0_load215 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr216 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load215, i32 0, i32 1
  %data_header217 = load ptr, ptr %df_data_ptr216, align 8
  %51 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header217, i32 0, i32 2
  %52 = load ptr, ptr %51, align 8
  %53 = getelementptr ptr, ptr %52, i64 12
  %54 = load ptr, ptr %53, align 8
  %__where_i_0_load218 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr219 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %54, i32 0, i32 0
  %data_field_ptr220 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %54, i32 0, i32 2
  %data_ptr221 = load ptr, ptr %data_field_ptr220, align 8
  %array_len222 = load i64, ptr %len_field_ptr219, align 8
  %index_is_neg223 = icmp slt i64 %__where_i_0_load218, 0
  %index_rel224 = add i64 %__where_i_0_load218, %array_len222
  %resolved_index225 = select i1 %index_is_neg223, i64 %index_rel224, i64 %__where_i_0_load218
  %is_neg226 = icmp slt i64 %resolved_index225, 0
  %is_too_big227 = icmp sge i64 %resolved_index225, %array_len222
  %is_invalid228 = or i1 %is_neg226, %is_too_big227
  br i1 %is_invalid228, label %bounds.fail229, label %bounds.ok230

bounds.fail229:                                   ; preds = %bounds.ok211
  %print_err231 = call i32 (ptr, ...) @printf(ptr @err_msg.12)
  ret ptr null

bounds.ok230:                                     ; preds = %bounds.ok211
  %elem_ptr232 = getelementptr double, ptr %data_ptr221, i64 %resolved_index225
  %loaded_val233 = load double, ptr %elem_ptr232, align 8
  %ptr_total-rainfall-sum = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 12
  store double %loaded_val233, ptr %ptr_total-rainfall-sum, align 8
  %__where_src_0_load234 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr235 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load234, i32 0, i32 1
  %data_header236 = load ptr, ptr %df_data_ptr235, align 8
  %55 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header236, i32 0, i32 2
  %56 = load ptr, ptr %55, align 8
  %57 = getelementptr ptr, ptr %56, i64 13
  %58 = load ptr, ptr %57, align 8
  %__where_i_0_load237 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr238 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %58, i32 0, i32 0
  %data_field_ptr239 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %58, i32 0, i32 2
  %data_ptr240 = load ptr, ptr %data_field_ptr239, align 8
  %array_len241 = load i64, ptr %len_field_ptr238, align 8
  %index_is_neg242 = icmp slt i64 %__where_i_0_load237, 0
  %index_rel243 = add i64 %__where_i_0_load237, %array_len241
  %resolved_index244 = select i1 %index_is_neg242, i64 %index_rel243, i64 %__where_i_0_load237
  %is_neg245 = icmp slt i64 %resolved_index244, 0
  %is_too_big246 = icmp sge i64 %resolved_index244, %array_len241
  %is_invalid247 = or i1 %is_neg245, %is_too_big246
  br i1 %is_invalid247, label %bounds.fail248, label %bounds.ok249

bounds.fail248:                                   ; preds = %bounds.ok230
  %print_err250 = call i32 (ptr, ...) @printf(ptr @err_msg.13)
  ret ptr null

bounds.ok249:                                     ; preds = %bounds.ok230
  %elem_ptr251 = getelementptr double, ptr %data_ptr240, i64 %resolved_index244
  %loaded_val252 = load double, ptr %elem_ptr251, align 8
  %ptr_surface-humidity-min = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 13
  store double %loaded_val252, ptr %ptr_surface-humidity-min, align 8
  %__where_src_0_load253 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr254 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load253, i32 0, i32 1
  %data_header255 = load ptr, ptr %df_data_ptr254, align 8
  %59 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header255, i32 0, i32 2
  %60 = load ptr, ptr %59, align 8
  %61 = getelementptr ptr, ptr %60, i64 14
  %62 = load ptr, ptr %61, align 8
  %__where_i_0_load256 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr257 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %62, i32 0, i32 0
  %data_field_ptr258 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %62, i32 0, i32 2
  %data_ptr259 = load ptr, ptr %data_field_ptr258, align 8
  %array_len260 = load i64, ptr %len_field_ptr257, align 8
  %index_is_neg261 = icmp slt i64 %__where_i_0_load256, 0
  %index_rel262 = add i64 %__where_i_0_load256, %array_len260
  %resolved_index263 = select i1 %index_is_neg261, i64 %index_rel262, i64 %__where_i_0_load256
  %is_neg264 = icmp slt i64 %resolved_index263, 0
  %is_too_big265 = icmp sge i64 %resolved_index263, %array_len260
  %is_invalid266 = or i1 %is_neg264, %is_too_big265
  br i1 %is_invalid266, label %bounds.fail267, label %bounds.ok268

bounds.fail267:                                   ; preds = %bounds.ok249
  %print_err269 = call i32 (ptr, ...) @printf(ptr @err_msg.14)
  ret ptr null

bounds.ok268:                                     ; preds = %bounds.ok249
  %elem_ptr270 = getelementptr double, ptr %data_ptr259, i64 %resolved_index263
  %loaded_val271 = load double, ptr %elem_ptr270, align 8
  %ptr_surface-humidity-max = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 14
  store double %loaded_val271, ptr %ptr_surface-humidity-max, align 8
  %__where_src_0_load272 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr273 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load272, i32 0, i32 1
  %data_header274 = load ptr, ptr %df_data_ptr273, align 8
  %63 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header274, i32 0, i32 2
  %64 = load ptr, ptr %63, align 8
  %65 = getelementptr ptr, ptr %64, i64 15
  %66 = load ptr, ptr %65, align 8
  %__where_i_0_load275 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr276 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %66, i32 0, i32 0
  %data_field_ptr277 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %66, i32 0, i32 2
  %data_ptr278 = load ptr, ptr %data_field_ptr277, align 8
  %array_len279 = load i64, ptr %len_field_ptr276, align 8
  %index_is_neg280 = icmp slt i64 %__where_i_0_load275, 0
  %index_rel281 = add i64 %__where_i_0_load275, %array_len279
  %resolved_index282 = select i1 %index_is_neg280, i64 %index_rel281, i64 %__where_i_0_load275
  %is_neg283 = icmp slt i64 %resolved_index282, 0
  %is_too_big284 = icmp sge i64 %resolved_index282, %array_len279
  %is_invalid285 = or i1 %is_neg283, %is_too_big284
  br i1 %is_invalid285, label %bounds.fail286, label %bounds.ok287

bounds.fail286:                                   ; preds = %bounds.ok268
  %print_err288 = call i32 (ptr, ...) @printf(ptr @err_msg.15)
  ret ptr null

bounds.ok287:                                     ; preds = %bounds.ok268
  %elem_ptr289 = getelementptr double, ptr %data_ptr278, i64 %resolved_index282
  %loaded_val290 = load double, ptr %elem_ptr289, align 8
  %ptr_surface-humidity-mean = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 15
  store double %loaded_val290, ptr %ptr_surface-humidity-mean, align 8
  %__where_src_0_load291 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr292 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load291, i32 0, i32 1
  %data_header293 = load ptr, ptr %df_data_ptr292, align 8
  %67 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header293, i32 0, i32 2
  %68 = load ptr, ptr %67, align 8
  %69 = getelementptr ptr, ptr %68, i64 16
  %70 = load ptr, ptr %69, align 8
  %__where_i_0_load294 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr295 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %70, i32 0, i32 0
  %data_field_ptr296 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %70, i32 0, i32 2
  %data_ptr297 = load ptr, ptr %data_field_ptr296, align 8
  %array_len298 = load i64, ptr %len_field_ptr295, align 8
  %index_is_neg299 = icmp slt i64 %__where_i_0_load294, 0
  %index_rel300 = add i64 %__where_i_0_load294, %array_len298
  %resolved_index301 = select i1 %index_is_neg299, i64 %index_rel300, i64 %__where_i_0_load294
  %is_neg302 = icmp slt i64 %resolved_index301, 0
  %is_too_big303 = icmp sge i64 %resolved_index301, %array_len298
  %is_invalid304 = or i1 %is_neg302, %is_too_big303
  br i1 %is_invalid304, label %bounds.fail305, label %bounds.ok306

bounds.fail305:                                   ; preds = %bounds.ok287
  %print_err307 = call i32 (ptr, ...) @printf(ptr @err_msg.16)
  ret ptr null

bounds.ok306:                                     ; preds = %bounds.ok287
  %elem_ptr308 = getelementptr double, ptr %data_ptr297, i64 %resolved_index301
  %loaded_val309 = load double, ptr %elem_ptr308, align 8
  %ptr_ndvi = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 16
  store double %loaded_val309, ptr %ptr_ndvi, align 8
  %__where_src_0_load310 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr311 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load310, i32 0, i32 1
  %data_header312 = load ptr, ptr %df_data_ptr311, align 8
  %71 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header312, i32 0, i32 2
  %72 = load ptr, ptr %71, align 8
  %73 = getelementptr ptr, ptr %72, i64 17
  %74 = load ptr, ptr %73, align 8
  %__where_i_0_load313 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr314 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %74, i32 0, i32 0
  %data_field_ptr315 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %74, i32 0, i32 2
  %data_ptr316 = load ptr, ptr %data_field_ptr315, align 8
  %array_len317 = load i64, ptr %len_field_ptr314, align 8
  %index_is_neg318 = icmp slt i64 %__where_i_0_load313, 0
  %index_rel319 = add i64 %__where_i_0_load313, %array_len317
  %resolved_index320 = select i1 %index_is_neg318, i64 %index_rel319, i64 %__where_i_0_load313
  %is_neg321 = icmp slt i64 %resolved_index320, 0
  %is_too_big322 = icmp sge i64 %resolved_index320, %array_len317
  %is_invalid323 = or i1 %is_neg321, %is_too_big322
  br i1 %is_invalid323, label %bounds.fail324, label %bounds.ok325

bounds.fail324:                                   ; preds = %bounds.ok306
  %print_err326 = call i32 (ptr, ...) @printf(ptr @err_msg.17)
  ret ptr null

bounds.ok325:                                     ; preds = %bounds.ok306
  %elem_ptr327 = getelementptr double, ptr %data_ptr316, i64 %resolved_index320
  %loaded_val328 = load double, ptr %elem_ptr327, align 8
  %ptr_elevation = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 17
  store double %loaded_val328, ptr %ptr_elevation, align 8
  %__where_src_0_load329 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr330 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load329, i32 0, i32 1
  %data_header331 = load ptr, ptr %df_data_ptr330, align 8
  %75 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header331, i32 0, i32 2
  %76 = load ptr, ptr %75, align 8
  %77 = getelementptr ptr, ptr %76, i64 18
  %78 = load ptr, ptr %77, align 8
  %__where_i_0_load332 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr333 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %78, i32 0, i32 0
  %data_field_ptr334 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %78, i32 0, i32 2
  %data_ptr335 = load ptr, ptr %data_field_ptr334, align 8
  %array_len336 = load i64, ptr %len_field_ptr333, align 8
  %index_is_neg337 = icmp slt i64 %__where_i_0_load332, 0
  %index_rel338 = add i64 %__where_i_0_load332, %array_len336
  %resolved_index339 = select i1 %index_is_neg337, i64 %index_rel338, i64 %__where_i_0_load332
  %is_neg340 = icmp slt i64 %resolved_index339, 0
  %is_too_big341 = icmp sge i64 %resolved_index339, %array_len336
  %is_invalid342 = or i1 %is_neg340, %is_too_big341
  br i1 %is_invalid342, label %bounds.fail343, label %bounds.ok344

bounds.fail343:                                   ; preds = %bounds.ok325
  %print_err345 = call i32 (ptr, ...) @printf(ptr @err_msg.18)
  ret ptr null

bounds.ok344:                                     ; preds = %bounds.ok325
  %elem_ptr346 = getelementptr double, ptr %data_ptr335, i64 %resolved_index339
  %loaded_val347 = load double, ptr %elem_ptr346, align 8
  %ptr_slope = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 18
  store double %loaded_val347, ptr %ptr_slope, align 8
  %__where_src_0_load348 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr349 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load348, i32 0, i32 1
  %data_header350 = load ptr, ptr %df_data_ptr349, align 8
  %79 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header350, i32 0, i32 2
  %80 = load ptr, ptr %79, align 8
  %81 = getelementptr ptr, ptr %80, i64 19
  %82 = load ptr, ptr %81, align 8
  %__where_i_0_load351 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr352 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %82, i32 0, i32 0
  %data_field_ptr353 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %82, i32 0, i32 2
  %data_ptr354 = load ptr, ptr %data_field_ptr353, align 8
  %array_len355 = load i64, ptr %len_field_ptr352, align 8
  %index_is_neg356 = icmp slt i64 %__where_i_0_load351, 0
  %index_rel357 = add i64 %__where_i_0_load351, %array_len355
  %resolved_index358 = select i1 %index_is_neg356, i64 %index_rel357, i64 %__where_i_0_load351
  %is_neg359 = icmp slt i64 %resolved_index358, 0
  %is_too_big360 = icmp sge i64 %resolved_index358, %array_len355
  %is_invalid361 = or i1 %is_neg359, %is_too_big360
  br i1 %is_invalid361, label %bounds.fail362, label %bounds.ok363

bounds.fail362:                                   ; preds = %bounds.ok344
  %print_err364 = call i32 (ptr, ...) @printf(ptr @err_msg.19)
  ret ptr null

bounds.ok363:                                     ; preds = %bounds.ok344
  %elem_ptr365 = getelementptr double, ptr %data_ptr354, i64 %resolved_index358
  %loaded_val366 = load double, ptr %elem_ptr365, align 8
  %ptr_aspect = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 19
  store double %loaded_val366, ptr %ptr_aspect, align 8
  %__where_src_0_load367 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr368 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load367, i32 0, i32 1
  %data_header369 = load ptr, ptr %df_data_ptr368, align 8
  %83 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header369, i32 0, i32 2
  %84 = load ptr, ptr %83, align 8
  %85 = getelementptr ptr, ptr %84, i64 20
  %86 = load ptr, ptr %85, align 8
  %__where_i_0_load370 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr371 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %86, i32 0, i32 0
  %data_field_ptr372 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %86, i32 0, i32 2
  %data_ptr373 = load ptr, ptr %data_field_ptr372, align 8
  %array_len374 = load i64, ptr %len_field_ptr371, align 8
  %index_is_neg375 = icmp slt i64 %__where_i_0_load370, 0
  %index_rel376 = add i64 %__where_i_0_load370, %array_len374
  %resolved_index377 = select i1 %index_is_neg375, i64 %index_rel376, i64 %__where_i_0_load370
  %is_neg378 = icmp slt i64 %resolved_index377, 0
  %is_too_big379 = icmp sge i64 %resolved_index377, %array_len374
  %is_invalid380 = or i1 %is_neg378, %is_too_big379
  br i1 %is_invalid380, label %bounds.fail381, label %bounds.ok382

bounds.fail381:                                   ; preds = %bounds.ok363
  %print_err383 = call i32 (ptr, ...) @printf(ptr @err_msg.20)
  ret ptr null

bounds.ok382:                                     ; preds = %bounds.ok363
  %elem_ptr384 = getelementptr i64, ptr %data_ptr373, i64 %resolved_index377
  %loaded_val385 = load i64, ptr %elem_ptr384, align 8
  %ptr_fire_label = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 20
  store i64 %loaded_val385, ptr %ptr_fire_label, align 4
  %__where_src_0_load386 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr387 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load386, i32 0, i32 1
  %data_header388 = load ptr, ptr %df_data_ptr387, align 8
  %87 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header388, i32 0, i32 2
  %88 = load ptr, ptr %87, align 8
  %89 = getelementptr ptr, ptr %88, i64 21
  %90 = load ptr, ptr %89, align 8
  %__where_i_0_load389 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr390 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %90, i32 0, i32 0
  %data_field_ptr391 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %90, i32 0, i32 2
  %data_ptr392 = load ptr, ptr %data_field_ptr391, align 8
  %array_len393 = load i64, ptr %len_field_ptr390, align 8
  %index_is_neg394 = icmp slt i64 %__where_i_0_load389, 0
  %index_rel395 = add i64 %__where_i_0_load389, %array_len393
  %resolved_index396 = select i1 %index_is_neg394, i64 %index_rel395, i64 %__where_i_0_load389
  %is_neg397 = icmp slt i64 %resolved_index396, 0
  %is_too_big398 = icmp sge i64 %resolved_index396, %array_len393
  %is_invalid399 = or i1 %is_neg397, %is_too_big398
  br i1 %is_invalid399, label %bounds.fail400, label %bounds.ok401

bounds.fail400:                                   ; preds = %bounds.ok382
  %print_err402 = call i32 (ptr, ...) @printf(ptr @err_msg.21)
  ret ptr null

bounds.ok401:                                     ; preds = %bounds.ok382
  %elem_ptr403 = getelementptr i8, ptr %data_ptr392, i64 %resolved_index396
  %loaded_val404 = load i8, ptr %elem_ptr403, align 1
  %ptr_land_cover_class_1 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 21
  store i8 %loaded_val404, ptr %ptr_land_cover_class_1, align 1
  %__where_src_0_load405 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr406 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load405, i32 0, i32 1
  %data_header407 = load ptr, ptr %df_data_ptr406, align 8
  %91 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header407, i32 0, i32 2
  %92 = load ptr, ptr %91, align 8
  %93 = getelementptr ptr, ptr %92, i64 22
  %94 = load ptr, ptr %93, align 8
  %__where_i_0_load408 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr409 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %94, i32 0, i32 0
  %data_field_ptr410 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %94, i32 0, i32 2
  %data_ptr411 = load ptr, ptr %data_field_ptr410, align 8
  %array_len412 = load i64, ptr %len_field_ptr409, align 8
  %index_is_neg413 = icmp slt i64 %__where_i_0_load408, 0
  %index_rel414 = add i64 %__where_i_0_load408, %array_len412
  %resolved_index415 = select i1 %index_is_neg413, i64 %index_rel414, i64 %__where_i_0_load408
  %is_neg416 = icmp slt i64 %resolved_index415, 0
  %is_too_big417 = icmp sge i64 %resolved_index415, %array_len412
  %is_invalid418 = or i1 %is_neg416, %is_too_big417
  br i1 %is_invalid418, label %bounds.fail419, label %bounds.ok420

bounds.fail419:                                   ; preds = %bounds.ok401
  %print_err421 = call i32 (ptr, ...) @printf(ptr @err_msg.22)
  ret ptr null

bounds.ok420:                                     ; preds = %bounds.ok401
  %elem_ptr422 = getelementptr i8, ptr %data_ptr411, i64 %resolved_index415
  %loaded_val423 = load i8, ptr %elem_ptr422, align 1
  %ptr_land_cover_class_2 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 22
  store i8 %loaded_val423, ptr %ptr_land_cover_class_2, align 1
  %__where_src_0_load424 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr425 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load424, i32 0, i32 1
  %data_header426 = load ptr, ptr %df_data_ptr425, align 8
  %95 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header426, i32 0, i32 2
  %96 = load ptr, ptr %95, align 8
  %97 = getelementptr ptr, ptr %96, i64 23
  %98 = load ptr, ptr %97, align 8
  %__where_i_0_load427 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr428 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %98, i32 0, i32 0
  %data_field_ptr429 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %98, i32 0, i32 2
  %data_ptr430 = load ptr, ptr %data_field_ptr429, align 8
  %array_len431 = load i64, ptr %len_field_ptr428, align 8
  %index_is_neg432 = icmp slt i64 %__where_i_0_load427, 0
  %index_rel433 = add i64 %__where_i_0_load427, %array_len431
  %resolved_index434 = select i1 %index_is_neg432, i64 %index_rel433, i64 %__where_i_0_load427
  %is_neg435 = icmp slt i64 %resolved_index434, 0
  %is_too_big436 = icmp sge i64 %resolved_index434, %array_len431
  %is_invalid437 = or i1 %is_neg435, %is_too_big436
  br i1 %is_invalid437, label %bounds.fail438, label %bounds.ok439

bounds.fail438:                                   ; preds = %bounds.ok420
  %print_err440 = call i32 (ptr, ...) @printf(ptr @err_msg.23)
  ret ptr null

bounds.ok439:                                     ; preds = %bounds.ok420
  %elem_ptr441 = getelementptr i8, ptr %data_ptr430, i64 %resolved_index434
  %loaded_val442 = load i8, ptr %elem_ptr441, align 1
  %ptr_land_cover_class_4 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 23
  store i8 %loaded_val442, ptr %ptr_land_cover_class_4, align 1
  %__where_src_0_load443 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr444 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load443, i32 0, i32 1
  %data_header445 = load ptr, ptr %df_data_ptr444, align 8
  %99 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header445, i32 0, i32 2
  %100 = load ptr, ptr %99, align 8
  %101 = getelementptr ptr, ptr %100, i64 24
  %102 = load ptr, ptr %101, align 8
  %__where_i_0_load446 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr447 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %102, i32 0, i32 0
  %data_field_ptr448 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %102, i32 0, i32 2
  %data_ptr449 = load ptr, ptr %data_field_ptr448, align 8
  %array_len450 = load i64, ptr %len_field_ptr447, align 8
  %index_is_neg451 = icmp slt i64 %__where_i_0_load446, 0
  %index_rel452 = add i64 %__where_i_0_load446, %array_len450
  %resolved_index453 = select i1 %index_is_neg451, i64 %index_rel452, i64 %__where_i_0_load446
  %is_neg454 = icmp slt i64 %resolved_index453, 0
  %is_too_big455 = icmp sge i64 %resolved_index453, %array_len450
  %is_invalid456 = or i1 %is_neg454, %is_too_big455
  br i1 %is_invalid456, label %bounds.fail457, label %bounds.ok458

bounds.fail457:                                   ; preds = %bounds.ok439
  %print_err459 = call i32 (ptr, ...) @printf(ptr @err_msg.24)
  ret ptr null

bounds.ok458:                                     ; preds = %bounds.ok439
  %elem_ptr460 = getelementptr i8, ptr %data_ptr449, i64 %resolved_index453
  %loaded_val461 = load i8, ptr %elem_ptr460, align 1
  %ptr_land_cover_class_5 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 24
  store i8 %loaded_val461, ptr %ptr_land_cover_class_5, align 1
  %__where_src_0_load462 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr463 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load462, i32 0, i32 1
  %data_header464 = load ptr, ptr %df_data_ptr463, align 8
  %103 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header464, i32 0, i32 2
  %104 = load ptr, ptr %103, align 8
  %105 = getelementptr ptr, ptr %104, i64 25
  %106 = load ptr, ptr %105, align 8
  %__where_i_0_load465 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr466 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %106, i32 0, i32 0
  %data_field_ptr467 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %106, i32 0, i32 2
  %data_ptr468 = load ptr, ptr %data_field_ptr467, align 8
  %array_len469 = load i64, ptr %len_field_ptr466, align 8
  %index_is_neg470 = icmp slt i64 %__where_i_0_load465, 0
  %index_rel471 = add i64 %__where_i_0_load465, %array_len469
  %resolved_index472 = select i1 %index_is_neg470, i64 %index_rel471, i64 %__where_i_0_load465
  %is_neg473 = icmp slt i64 %resolved_index472, 0
  %is_too_big474 = icmp sge i64 %resolved_index472, %array_len469
  %is_invalid475 = or i1 %is_neg473, %is_too_big474
  br i1 %is_invalid475, label %bounds.fail476, label %bounds.ok477

bounds.fail476:                                   ; preds = %bounds.ok458
  %print_err478 = call i32 (ptr, ...) @printf(ptr @err_msg.25)
  ret ptr null

bounds.ok477:                                     ; preds = %bounds.ok458
  %elem_ptr479 = getelementptr i8, ptr %data_ptr468, i64 %resolved_index472
  %loaded_val480 = load i8, ptr %elem_ptr479, align 1
  %ptr_land_cover_class_6 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 25
  store i8 %loaded_val480, ptr %ptr_land_cover_class_6, align 1
  %__where_src_0_load481 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr482 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load481, i32 0, i32 1
  %data_header483 = load ptr, ptr %df_data_ptr482, align 8
  %107 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header483, i32 0, i32 2
  %108 = load ptr, ptr %107, align 8
  %109 = getelementptr ptr, ptr %108, i64 26
  %110 = load ptr, ptr %109, align 8
  %__where_i_0_load484 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr485 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %110, i32 0, i32 0
  %data_field_ptr486 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %110, i32 0, i32 2
  %data_ptr487 = load ptr, ptr %data_field_ptr486, align 8
  %array_len488 = load i64, ptr %len_field_ptr485, align 8
  %index_is_neg489 = icmp slt i64 %__where_i_0_load484, 0
  %index_rel490 = add i64 %__where_i_0_load484, %array_len488
  %resolved_index491 = select i1 %index_is_neg489, i64 %index_rel490, i64 %__where_i_0_load484
  %is_neg492 = icmp slt i64 %resolved_index491, 0
  %is_too_big493 = icmp sge i64 %resolved_index491, %array_len488
  %is_invalid494 = or i1 %is_neg492, %is_too_big493
  br i1 %is_invalid494, label %bounds.fail495, label %bounds.ok496

bounds.fail495:                                   ; preds = %bounds.ok477
  %print_err497 = call i32 (ptr, ...) @printf(ptr @err_msg.26)
  ret ptr null

bounds.ok496:                                     ; preds = %bounds.ok477
  %elem_ptr498 = getelementptr i8, ptr %data_ptr487, i64 %resolved_index491
  %loaded_val499 = load i8, ptr %elem_ptr498, align 1
  %ptr_land_cover_class_7 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 26
  store i8 %loaded_val499, ptr %ptr_land_cover_class_7, align 1
  %__where_src_0_load500 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr501 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load500, i32 0, i32 1
  %data_header502 = load ptr, ptr %df_data_ptr501, align 8
  %111 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header502, i32 0, i32 2
  %112 = load ptr, ptr %111, align 8
  %113 = getelementptr ptr, ptr %112, i64 27
  %114 = load ptr, ptr %113, align 8
  %__where_i_0_load503 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr504 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %114, i32 0, i32 0
  %data_field_ptr505 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %114, i32 0, i32 2
  %data_ptr506 = load ptr, ptr %data_field_ptr505, align 8
  %array_len507 = load i64, ptr %len_field_ptr504, align 8
  %index_is_neg508 = icmp slt i64 %__where_i_0_load503, 0
  %index_rel509 = add i64 %__where_i_0_load503, %array_len507
  %resolved_index510 = select i1 %index_is_neg508, i64 %index_rel509, i64 %__where_i_0_load503
  %is_neg511 = icmp slt i64 %resolved_index510, 0
  %is_too_big512 = icmp sge i64 %resolved_index510, %array_len507
  %is_invalid513 = or i1 %is_neg511, %is_too_big512
  br i1 %is_invalid513, label %bounds.fail514, label %bounds.ok515

bounds.fail514:                                   ; preds = %bounds.ok496
  %print_err516 = call i32 (ptr, ...) @printf(ptr @err_msg.27)
  ret ptr null

bounds.ok515:                                     ; preds = %bounds.ok496
  %elem_ptr517 = getelementptr i8, ptr %data_ptr506, i64 %resolved_index510
  %loaded_val518 = load i8, ptr %elem_ptr517, align 1
  %ptr_land_cover_class_8 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 27
  store i8 %loaded_val518, ptr %ptr_land_cover_class_8, align 1
  %__where_src_0_load519 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr520 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load519, i32 0, i32 1
  %data_header521 = load ptr, ptr %df_data_ptr520, align 8
  %115 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header521, i32 0, i32 2
  %116 = load ptr, ptr %115, align 8
  %117 = getelementptr ptr, ptr %116, i64 28
  %118 = load ptr, ptr %117, align 8
  %__where_i_0_load522 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr523 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %118, i32 0, i32 0
  %data_field_ptr524 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %118, i32 0, i32 2
  %data_ptr525 = load ptr, ptr %data_field_ptr524, align 8
  %array_len526 = load i64, ptr %len_field_ptr523, align 8
  %index_is_neg527 = icmp slt i64 %__where_i_0_load522, 0
  %index_rel528 = add i64 %__where_i_0_load522, %array_len526
  %resolved_index529 = select i1 %index_is_neg527, i64 %index_rel528, i64 %__where_i_0_load522
  %is_neg530 = icmp slt i64 %resolved_index529, 0
  %is_too_big531 = icmp sge i64 %resolved_index529, %array_len526
  %is_invalid532 = or i1 %is_neg530, %is_too_big531
  br i1 %is_invalid532, label %bounds.fail533, label %bounds.ok534

bounds.fail533:                                   ; preds = %bounds.ok515
  %print_err535 = call i32 (ptr, ...) @printf(ptr @err_msg.28)
  ret ptr null

bounds.ok534:                                     ; preds = %bounds.ok515
  %elem_ptr536 = getelementptr i8, ptr %data_ptr525, i64 %resolved_index529
  %loaded_val537 = load i8, ptr %elem_ptr536, align 1
  %ptr_land_cover_class_9 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 28
  store i8 %loaded_val537, ptr %ptr_land_cover_class_9, align 1
  %__where_src_0_load538 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr539 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load538, i32 0, i32 1
  %data_header540 = load ptr, ptr %df_data_ptr539, align 8
  %119 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header540, i32 0, i32 2
  %120 = load ptr, ptr %119, align 8
  %121 = getelementptr ptr, ptr %120, i64 29
  %122 = load ptr, ptr %121, align 8
  %__where_i_0_load541 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr542 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %122, i32 0, i32 0
  %data_field_ptr543 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %122, i32 0, i32 2
  %data_ptr544 = load ptr, ptr %data_field_ptr543, align 8
  %array_len545 = load i64, ptr %len_field_ptr542, align 8
  %index_is_neg546 = icmp slt i64 %__where_i_0_load541, 0
  %index_rel547 = add i64 %__where_i_0_load541, %array_len545
  %resolved_index548 = select i1 %index_is_neg546, i64 %index_rel547, i64 %__where_i_0_load541
  %is_neg549 = icmp slt i64 %resolved_index548, 0
  %is_too_big550 = icmp sge i64 %resolved_index548, %array_len545
  %is_invalid551 = or i1 %is_neg549, %is_too_big550
  br i1 %is_invalid551, label %bounds.fail552, label %bounds.ok553

bounds.fail552:                                   ; preds = %bounds.ok534
  %print_err554 = call i32 (ptr, ...) @printf(ptr @err_msg.29)
  ret ptr null

bounds.ok553:                                     ; preds = %bounds.ok534
  %elem_ptr555 = getelementptr i8, ptr %data_ptr544, i64 %resolved_index548
  %loaded_val556 = load i8, ptr %elem_ptr555, align 1
  %ptr_land_cover_class_10 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 29
  store i8 %loaded_val556, ptr %ptr_land_cover_class_10, align 1
  %__where_src_0_load557 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr558 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load557, i32 0, i32 1
  %data_header559 = load ptr, ptr %df_data_ptr558, align 8
  %123 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header559, i32 0, i32 2
  %124 = load ptr, ptr %123, align 8
  %125 = getelementptr ptr, ptr %124, i64 30
  %126 = load ptr, ptr %125, align 8
  %__where_i_0_load560 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr561 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %126, i32 0, i32 0
  %data_field_ptr562 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %126, i32 0, i32 2
  %data_ptr563 = load ptr, ptr %data_field_ptr562, align 8
  %array_len564 = load i64, ptr %len_field_ptr561, align 8
  %index_is_neg565 = icmp slt i64 %__where_i_0_load560, 0
  %index_rel566 = add i64 %__where_i_0_load560, %array_len564
  %resolved_index567 = select i1 %index_is_neg565, i64 %index_rel566, i64 %__where_i_0_load560
  %is_neg568 = icmp slt i64 %resolved_index567, 0
  %is_too_big569 = icmp sge i64 %resolved_index567, %array_len564
  %is_invalid570 = or i1 %is_neg568, %is_too_big569
  br i1 %is_invalid570, label %bounds.fail571, label %bounds.ok572

bounds.fail571:                                   ; preds = %bounds.ok553
  %print_err573 = call i32 (ptr, ...) @printf(ptr @err_msg.30)
  ret ptr null

bounds.ok572:                                     ; preds = %bounds.ok553
  %elem_ptr574 = getelementptr i8, ptr %data_ptr563, i64 %resolved_index567
  %loaded_val575 = load i8, ptr %elem_ptr574, align 1
  %ptr_land_cover_class_11 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 30
  store i8 %loaded_val575, ptr %ptr_land_cover_class_11, align 1
  %__where_src_0_load576 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr577 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load576, i32 0, i32 1
  %data_header578 = load ptr, ptr %df_data_ptr577, align 8
  %127 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header578, i32 0, i32 2
  %128 = load ptr, ptr %127, align 8
  %129 = getelementptr ptr, ptr %128, i64 31
  %130 = load ptr, ptr %129, align 8
  %__where_i_0_load579 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr580 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %130, i32 0, i32 0
  %data_field_ptr581 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %130, i32 0, i32 2
  %data_ptr582 = load ptr, ptr %data_field_ptr581, align 8
  %array_len583 = load i64, ptr %len_field_ptr580, align 8
  %index_is_neg584 = icmp slt i64 %__where_i_0_load579, 0
  %index_rel585 = add i64 %__where_i_0_load579, %array_len583
  %resolved_index586 = select i1 %index_is_neg584, i64 %index_rel585, i64 %__where_i_0_load579
  %is_neg587 = icmp slt i64 %resolved_index586, 0
  %is_too_big588 = icmp sge i64 %resolved_index586, %array_len583
  %is_invalid589 = or i1 %is_neg587, %is_too_big588
  br i1 %is_invalid589, label %bounds.fail590, label %bounds.ok591

bounds.fail590:                                   ; preds = %bounds.ok572
  %print_err592 = call i32 (ptr, ...) @printf(ptr @err_msg.31)
  ret ptr null

bounds.ok591:                                     ; preds = %bounds.ok572
  %elem_ptr593 = getelementptr i8, ptr %data_ptr582, i64 %resolved_index586
  %loaded_val594 = load i8, ptr %elem_ptr593, align 1
  %ptr_land_cover_class_12 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 31
  store i8 %loaded_val594, ptr %ptr_land_cover_class_12, align 1
  %__where_src_0_load595 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr596 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load595, i32 0, i32 1
  %data_header597 = load ptr, ptr %df_data_ptr596, align 8
  %131 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header597, i32 0, i32 2
  %132 = load ptr, ptr %131, align 8
  %133 = getelementptr ptr, ptr %132, i64 32
  %134 = load ptr, ptr %133, align 8
  %__where_i_0_load598 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr599 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %134, i32 0, i32 0
  %data_field_ptr600 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %134, i32 0, i32 2
  %data_ptr601 = load ptr, ptr %data_field_ptr600, align 8
  %array_len602 = load i64, ptr %len_field_ptr599, align 8
  %index_is_neg603 = icmp slt i64 %__where_i_0_load598, 0
  %index_rel604 = add i64 %__where_i_0_load598, %array_len602
  %resolved_index605 = select i1 %index_is_neg603, i64 %index_rel604, i64 %__where_i_0_load598
  %is_neg606 = icmp slt i64 %resolved_index605, 0
  %is_too_big607 = icmp sge i64 %resolved_index605, %array_len602
  %is_invalid608 = or i1 %is_neg606, %is_too_big607
  br i1 %is_invalid608, label %bounds.fail609, label %bounds.ok610

bounds.fail609:                                   ; preds = %bounds.ok591
  %print_err611 = call i32 (ptr, ...) @printf(ptr @err_msg.32)
  ret ptr null

bounds.ok610:                                     ; preds = %bounds.ok591
  %elem_ptr612 = getelementptr i8, ptr %data_ptr601, i64 %resolved_index605
  %loaded_val613 = load i8, ptr %elem_ptr612, align 1
  %ptr_land_cover_class_13 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 32
  store i8 %loaded_val613, ptr %ptr_land_cover_class_13, align 1
  %__where_src_0_load614 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr615 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load614, i32 0, i32 1
  %data_header616 = load ptr, ptr %df_data_ptr615, align 8
  %135 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header616, i32 0, i32 2
  %136 = load ptr, ptr %135, align 8
  %137 = getelementptr ptr, ptr %136, i64 33
  %138 = load ptr, ptr %137, align 8
  %__where_i_0_load617 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr618 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %138, i32 0, i32 0
  %data_field_ptr619 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %138, i32 0, i32 2
  %data_ptr620 = load ptr, ptr %data_field_ptr619, align 8
  %array_len621 = load i64, ptr %len_field_ptr618, align 8
  %index_is_neg622 = icmp slt i64 %__where_i_0_load617, 0
  %index_rel623 = add i64 %__where_i_0_load617, %array_len621
  %resolved_index624 = select i1 %index_is_neg622, i64 %index_rel623, i64 %__where_i_0_load617
  %is_neg625 = icmp slt i64 %resolved_index624, 0
  %is_too_big626 = icmp sge i64 %resolved_index624, %array_len621
  %is_invalid627 = or i1 %is_neg625, %is_too_big626
  br i1 %is_invalid627, label %bounds.fail628, label %bounds.ok629

bounds.fail628:                                   ; preds = %bounds.ok610
  %print_err630 = call i32 (ptr, ...) @printf(ptr @err_msg.33)
  ret ptr null

bounds.ok629:                                     ; preds = %bounds.ok610
  %elem_ptr631 = getelementptr i8, ptr %data_ptr620, i64 %resolved_index624
  %loaded_val632 = load i8, ptr %elem_ptr631, align 1
  %ptr_land_cover_class_14 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 33
  store i8 %loaded_val632, ptr %ptr_land_cover_class_14, align 1
  %__where_src_0_load633 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr634 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load633, i32 0, i32 1
  %data_header635 = load ptr, ptr %df_data_ptr634, align 8
  %139 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header635, i32 0, i32 2
  %140 = load ptr, ptr %139, align 8
  %141 = getelementptr ptr, ptr %140, i64 34
  %142 = load ptr, ptr %141, align 8
  %__where_i_0_load636 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr637 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %142, i32 0, i32 0
  %data_field_ptr638 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %142, i32 0, i32 2
  %data_ptr639 = load ptr, ptr %data_field_ptr638, align 8
  %array_len640 = load i64, ptr %len_field_ptr637, align 8
  %index_is_neg641 = icmp slt i64 %__where_i_0_load636, 0
  %index_rel642 = add i64 %__where_i_0_load636, %array_len640
  %resolved_index643 = select i1 %index_is_neg641, i64 %index_rel642, i64 %__where_i_0_load636
  %is_neg644 = icmp slt i64 %resolved_index643, 0
  %is_too_big645 = icmp sge i64 %resolved_index643, %array_len640
  %is_invalid646 = or i1 %is_neg644, %is_too_big645
  br i1 %is_invalid646, label %bounds.fail647, label %bounds.ok648

bounds.fail647:                                   ; preds = %bounds.ok629
  %print_err649 = call i32 (ptr, ...) @printf(ptr @err_msg.34)
  ret ptr null

bounds.ok648:                                     ; preds = %bounds.ok629
  %elem_ptr650 = getelementptr i8, ptr %data_ptr639, i64 %resolved_index643
  %loaded_val651 = load i8, ptr %elem_ptr650, align 1
  %ptr_land_cover_class_15 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 34
  store i8 %loaded_val651, ptr %ptr_land_cover_class_15, align 1
  %__where_src_0_load652 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr653 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load652, i32 0, i32 1
  %data_header654 = load ptr, ptr %df_data_ptr653, align 8
  %143 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header654, i32 0, i32 2
  %144 = load ptr, ptr %143, align 8
  %145 = getelementptr ptr, ptr %144, i64 35
  %146 = load ptr, ptr %145, align 8
  %__where_i_0_load655 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr656 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %146, i32 0, i32 0
  %data_field_ptr657 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %146, i32 0, i32 2
  %data_ptr658 = load ptr, ptr %data_field_ptr657, align 8
  %array_len659 = load i64, ptr %len_field_ptr656, align 8
  %index_is_neg660 = icmp slt i64 %__where_i_0_load655, 0
  %index_rel661 = add i64 %__where_i_0_load655, %array_len659
  %resolved_index662 = select i1 %index_is_neg660, i64 %index_rel661, i64 %__where_i_0_load655
  %is_neg663 = icmp slt i64 %resolved_index662, 0
  %is_too_big664 = icmp sge i64 %resolved_index662, %array_len659
  %is_invalid665 = or i1 %is_neg663, %is_too_big664
  br i1 %is_invalid665, label %bounds.fail666, label %bounds.ok667

bounds.fail666:                                   ; preds = %bounds.ok648
  %print_err668 = call i32 (ptr, ...) @printf(ptr @err_msg.35)
  ret ptr null

bounds.ok667:                                     ; preds = %bounds.ok648
  %elem_ptr669 = getelementptr i8, ptr %data_ptr658, i64 %resolved_index662
  %loaded_val670 = load i8, ptr %elem_ptr669, align 1
  %ptr_land_cover_class_16 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 35
  store i8 %loaded_val670, ptr %ptr_land_cover_class_16, align 1
  %__where_src_0_load671 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr672 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load671, i32 0, i32 1
  %data_header673 = load ptr, ptr %df_data_ptr672, align 8
  %147 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header673, i32 0, i32 2
  %148 = load ptr, ptr %147, align 8
  %149 = getelementptr ptr, ptr %148, i64 36
  %150 = load ptr, ptr %149, align 8
  %__where_i_0_load674 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr675 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %150, i32 0, i32 0
  %data_field_ptr676 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %150, i32 0, i32 2
  %data_ptr677 = load ptr, ptr %data_field_ptr676, align 8
  %array_len678 = load i64, ptr %len_field_ptr675, align 8
  %index_is_neg679 = icmp slt i64 %__where_i_0_load674, 0
  %index_rel680 = add i64 %__where_i_0_load674, %array_len678
  %resolved_index681 = select i1 %index_is_neg679, i64 %index_rel680, i64 %__where_i_0_load674
  %is_neg682 = icmp slt i64 %resolved_index681, 0
  %is_too_big683 = icmp sge i64 %resolved_index681, %array_len678
  %is_invalid684 = or i1 %is_neg682, %is_too_big683
  br i1 %is_invalid684, label %bounds.fail685, label %bounds.ok686

bounds.fail685:                                   ; preds = %bounds.ok667
  %print_err687 = call i32 (ptr, ...) @printf(ptr @err_msg.36)
  ret ptr null

bounds.ok686:                                     ; preds = %bounds.ok667
  %elem_ptr688 = getelementptr i8, ptr %data_ptr677, i64 %resolved_index681
  %loaded_val689 = load i8, ptr %elem_ptr688, align 1
  %ptr_land_cover_class_17 = getelementptr inbounds nuw %struct_dateStringType_latitudeFloatType_longitudeFloatType_wind-speed-minFloatType_wind-speed-maxFloatType_wind-speed-meanFloatType_wind-direction-minFloatType_wind-direction-maxFloatType_wind-direction-meanFloatType_surface-air-temperature-minFloatType_surface-air-temperature-maxFloatType_surface-air-temperature-meanFloatType_total-rainfall-sumFloatType_surface-humidity-minFloatType_surface-humidity-maxFloatType_surface-humidity-meanFloatType_ndviFloatType_elevationFloatType_slopeFloatType_aspectFloatType_fire_labelIntType_land_cover_class_1BoolType_land_cover_class_2BoolType_land_cover_class_4BoolType_land_cover_class_5BoolType_land_cover_class_6BoolType_land_cover_class_7BoolType_land_cover_class_8BoolType_land_cover_class_9BoolType_land_cover_class_10BoolType_land_cover_class_11BoolType_land_cover_class_12BoolType_land_cover_class_13BoolType_land_cover_class_14BoolType_land_cover_class_15BoolType_land_cover_class_16BoolType_land_cover_class_17BoolType, ptr %record_mem, i32 0, i32 36
  store i8 %loaded_val689, ptr %ptr_land_cover_class_17, align 1
  store ptr %record_mem, ptr @x, align 8
  %__where_mask_0_load690 = load ptr, ptr @__where_mask_0, align 8
  %__where_i_0_load691 = load i64, ptr @__where_i_0, align 8
  %len_ptr692 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_mask_0_load690, i32 0, i32 0
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_mask_0_load690, i32 0, i32 2
  %array_len693 = load i64, ptr %len_ptr692, align 4
  %data_ptr694 = load ptr, ptr %data_ptr_ptr, align 8
  %151 = icmp slt i64 %__where_i_0_load691, 0
  %152 = icmp sge i64 %__where_i_0_load691, %array_len693
  %is_invalid695 = or i1 %151, %152
  br i1 %is_invalid695, label %assign_bounds.fail, label %assign_bounds.ok

assign_bounds.fail:                               ; preds = %bounds.ok686
  %print_err696 = call i32 (ptr, ...) @printf(ptr @err_msg.37)
  ret ptr null

assign_bounds.ok:                                 ; preds = %bounds.ok686
  %x_load = load ptr, ptr @x, align 8
  %ptr_latitude697 = getelementptr inbounds nuw %struct_date_latitude_longitude_wind-speed-min_wind-speed-max_wind-speed-mean_wind-direction-min_wind-direction-max_wind-direction-mean_surface-air-temperature-min_surface-air-temperature-max_surface-air-temperature-mean_total-rainfall-sum_surface-humidity-min_surface-humidity-max_surface-humidity-mean_ndvi_elevation_slope_aspect_fire_label_land_cover_class_1_land_cover_class_2_land_cover_class_4_land_cover_class_5_land_cover_class_6_land_cover_class_7_land_cover_class_8_land_cover_class_9_land_cover_class_10_land_cover_class_11_land_cover_class_12_land_cover_class_13_land_cover_class_14_land_cover_class_15_land_cover_class_16_land_cover_class_17, ptr %x_load, i32 0, i32 1
  %val_latitude = load double, ptr %ptr_latitude697, align 8
  %fcmp_tmp = fcmp ogt double %val_latitude, -1.800000e+01
  %bool_to_i8 = zext i1 %fcmp_tmp to i8
  %elem_ptr698 = getelementptr i8, ptr %data_ptr694, i64 %__where_i_0_load691
  store i8 %bool_to_i8, ptr %elem_ptr698, align 1
  br label %for.step

for.cond700:                                      ; preds = %for.step702, %for.end
  %__where_i_0_load704 = load i64, ptr @__where_i_0, align 8
  %__where_src_0_load705 = load ptr, ptr @__where_src_0, align 8
  %rowCount_ptr706 = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %__where_src_0_load705, i32 0, i32 3
  %rowCount707 = load i64, ptr %rowCount_ptr706, align 8
  %icmp_tmp708 = icmp slt i64 %__where_i_0_load704, %rowCount707
  br i1 %icmp_tmp708, label %for.body701, label %for.end703

for.body701:                                      ; preds = %for.cond700
  %__where_mask_0_load709 = load ptr, ptr @__where_mask_0, align 8
  %__where_i_0_load710 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr711 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_mask_0_load709, i32 0, i32 0
  %data_field_ptr712 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_mask_0_load709, i32 0, i32 2
  %data_ptr713 = load ptr, ptr %data_field_ptr712, align 8
  %array_len714 = load i64, ptr %len_field_ptr711, align 8
  %index_is_neg715 = icmp slt i64 %__where_i_0_load710, 0
  %index_rel716 = add i64 %__where_i_0_load710, %array_len714
  %resolved_index717 = select i1 %index_is_neg715, i64 %index_rel716, i64 %__where_i_0_load710
  %is_neg718 = icmp slt i64 %resolved_index717, 0
  %is_too_big719 = icmp sge i64 %resolved_index717, %array_len714
  %is_invalid720 = or i1 %is_neg718, %is_too_big719
  br i1 %is_invalid720, label %bounds.fail721, label %bounds.ok722

for.step702:                                      ; preds = %ifcont
  %x_load728 = load i64, ptr @__where_i_0, align 8
  %inc_add729 = add i64 %x_load728, 1
  store i64 %inc_add729, ptr @__where_i_0, align 8
  br label %for.cond700, !llvm.loop !2

for.end703:                                       ; preds = %for.cond700
  %arr_header730 = call ptr @malloc(i64 24)
  %153 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header730, i32 0, i32 0
  %154 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header730, i32 0, i32 1
  %155 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header730, i32 0, i32 2
  store i64 0, ptr %153, align 8
  %__mask_count_0_load = load i64, ptr @__mask_count_0, align 8
  %multmp = mul i64 %__mask_count_0_load, 8
  %arr_data731 = call ptr @malloc(i64 %multmp)
  store i64 %__mask_count_0_load, ptr %154, align 8
  store ptr %arr_data731, ptr %155, align 8
  %arr_header732 = call ptr @malloc(i64 24)
  %156 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header732, i32 0, i32 0
  %157 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header732, i32 0, i32 1
  %158 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header732, i32 0, i32 2
  store i64 0, ptr %156, align 8
  %__mask_count_0_load733 = load i64, ptr @__mask_count_0, align 8
  %multmp734 = mul i64 %__mask_count_0_load733, 8
  %arr_data735 = call ptr @malloc(i64 %multmp734)
  store i64 %__mask_count_0_load733, ptr %157, align 8
  store ptr %arr_data735, ptr %158, align 8
  %arr_header736 = call ptr @malloc(i64 24)
  %159 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header736, i32 0, i32 0
  %160 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header736, i32 0, i32 1
  %161 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header736, i32 0, i32 2
  store i64 0, ptr %159, align 8
  %__mask_count_0_load737 = load i64, ptr @__mask_count_0, align 8
  %multmp738 = mul i64 %__mask_count_0_load737, 8
  %arr_data739 = call ptr @malloc(i64 %multmp738)
  store i64 %__mask_count_0_load737, ptr %160, align 8
  store ptr %arr_data739, ptr %161, align 8
  %arr_header740 = call ptr @malloc(i64 24)
  %162 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header740, i32 0, i32 0
  %163 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header740, i32 0, i32 1
  %164 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header740, i32 0, i32 2
  store i64 0, ptr %162, align 8
  %__mask_count_0_load741 = load i64, ptr @__mask_count_0, align 8
  %multmp742 = mul i64 %__mask_count_0_load741, 8
  %arr_data743 = call ptr @malloc(i64 %multmp742)
  store i64 %__mask_count_0_load741, ptr %163, align 8
  store ptr %arr_data743, ptr %164, align 8
  %arr_header744 = call ptr @malloc(i64 24)
  %165 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header744, i32 0, i32 0
  %166 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header744, i32 0, i32 1
  %167 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header744, i32 0, i32 2
  store i64 0, ptr %165, align 8
  %__mask_count_0_load745 = load i64, ptr @__mask_count_0, align 8
  %multmp746 = mul i64 %__mask_count_0_load745, 8
  %arr_data747 = call ptr @malloc(i64 %multmp746)
  store i64 %__mask_count_0_load745, ptr %166, align 8
  store ptr %arr_data747, ptr %167, align 8
  %arr_header748 = call ptr @malloc(i64 24)
  %168 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header748, i32 0, i32 0
  %169 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header748, i32 0, i32 1
  %170 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header748, i32 0, i32 2
  store i64 0, ptr %168, align 8
  %__mask_count_0_load749 = load i64, ptr @__mask_count_0, align 8
  %multmp750 = mul i64 %__mask_count_0_load749, 8
  %arr_data751 = call ptr @malloc(i64 %multmp750)
  store i64 %__mask_count_0_load749, ptr %169, align 8
  store ptr %arr_data751, ptr %170, align 8
  %arr_header752 = call ptr @malloc(i64 24)
  %171 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header752, i32 0, i32 0
  %172 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header752, i32 0, i32 1
  %173 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header752, i32 0, i32 2
  store i64 0, ptr %171, align 8
  %__mask_count_0_load753 = load i64, ptr @__mask_count_0, align 8
  %multmp754 = mul i64 %__mask_count_0_load753, 8
  %arr_data755 = call ptr @malloc(i64 %multmp754)
  store i64 %__mask_count_0_load753, ptr %172, align 8
  store ptr %arr_data755, ptr %173, align 8
  %arr_header756 = call ptr @malloc(i64 24)
  %174 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header756, i32 0, i32 0
  %175 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header756, i32 0, i32 1
  %176 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header756, i32 0, i32 2
  store i64 0, ptr %174, align 8
  %__mask_count_0_load757 = load i64, ptr @__mask_count_0, align 8
  %multmp758 = mul i64 %__mask_count_0_load757, 8
  %arr_data759 = call ptr @malloc(i64 %multmp758)
  store i64 %__mask_count_0_load757, ptr %175, align 8
  store ptr %arr_data759, ptr %176, align 8
  %arr_header760 = call ptr @malloc(i64 24)
  %177 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header760, i32 0, i32 0
  %178 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header760, i32 0, i32 1
  %179 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header760, i32 0, i32 2
  store i64 0, ptr %177, align 8
  %__mask_count_0_load761 = load i64, ptr @__mask_count_0, align 8
  %multmp762 = mul i64 %__mask_count_0_load761, 8
  %arr_data763 = call ptr @malloc(i64 %multmp762)
  store i64 %__mask_count_0_load761, ptr %178, align 8
  store ptr %arr_data763, ptr %179, align 8
  %arr_header764 = call ptr @malloc(i64 24)
  %180 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header764, i32 0, i32 0
  %181 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header764, i32 0, i32 1
  %182 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header764, i32 0, i32 2
  store i64 0, ptr %180, align 8
  %__mask_count_0_load765 = load i64, ptr @__mask_count_0, align 8
  %multmp766 = mul i64 %__mask_count_0_load765, 8
  %arr_data767 = call ptr @malloc(i64 %multmp766)
  store i64 %__mask_count_0_load765, ptr %181, align 8
  store ptr %arr_data767, ptr %182, align 8
  %arr_header768 = call ptr @malloc(i64 24)
  %183 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header768, i32 0, i32 0
  %184 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header768, i32 0, i32 1
  %185 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header768, i32 0, i32 2
  store i64 0, ptr %183, align 8
  %__mask_count_0_load769 = load i64, ptr @__mask_count_0, align 8
  %multmp770 = mul i64 %__mask_count_0_load769, 8
  %arr_data771 = call ptr @malloc(i64 %multmp770)
  store i64 %__mask_count_0_load769, ptr %184, align 8
  store ptr %arr_data771, ptr %185, align 8
  %arr_header772 = call ptr @malloc(i64 24)
  %186 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header772, i32 0, i32 0
  %187 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header772, i32 0, i32 1
  %188 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header772, i32 0, i32 2
  store i64 0, ptr %186, align 8
  %__mask_count_0_load773 = load i64, ptr @__mask_count_0, align 8
  %multmp774 = mul i64 %__mask_count_0_load773, 8
  %arr_data775 = call ptr @malloc(i64 %multmp774)
  store i64 %__mask_count_0_load773, ptr %187, align 8
  store ptr %arr_data775, ptr %188, align 8
  %arr_header776 = call ptr @malloc(i64 24)
  %189 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header776, i32 0, i32 0
  %190 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header776, i32 0, i32 1
  %191 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header776, i32 0, i32 2
  store i64 0, ptr %189, align 8
  %__mask_count_0_load777 = load i64, ptr @__mask_count_0, align 8
  %multmp778 = mul i64 %__mask_count_0_load777, 8
  %arr_data779 = call ptr @malloc(i64 %multmp778)
  store i64 %__mask_count_0_load777, ptr %190, align 8
  store ptr %arr_data779, ptr %191, align 8
  %arr_header780 = call ptr @malloc(i64 24)
  %192 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header780, i32 0, i32 0
  %193 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header780, i32 0, i32 1
  %194 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header780, i32 0, i32 2
  store i64 0, ptr %192, align 8
  %__mask_count_0_load781 = load i64, ptr @__mask_count_0, align 8
  %multmp782 = mul i64 %__mask_count_0_load781, 8
  %arr_data783 = call ptr @malloc(i64 %multmp782)
  store i64 %__mask_count_0_load781, ptr %193, align 8
  store ptr %arr_data783, ptr %194, align 8
  %arr_header784 = call ptr @malloc(i64 24)
  %195 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header784, i32 0, i32 0
  %196 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header784, i32 0, i32 1
  %197 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header784, i32 0, i32 2
  store i64 0, ptr %195, align 8
  %__mask_count_0_load785 = load i64, ptr @__mask_count_0, align 8
  %multmp786 = mul i64 %__mask_count_0_load785, 8
  %arr_data787 = call ptr @malloc(i64 %multmp786)
  store i64 %__mask_count_0_load785, ptr %196, align 8
  store ptr %arr_data787, ptr %197, align 8
  %arr_header788 = call ptr @malloc(i64 24)
  %198 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header788, i32 0, i32 0
  %199 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header788, i32 0, i32 1
  %200 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header788, i32 0, i32 2
  store i64 0, ptr %198, align 8
  %__mask_count_0_load789 = load i64, ptr @__mask_count_0, align 8
  %multmp790 = mul i64 %__mask_count_0_load789, 8
  %arr_data791 = call ptr @malloc(i64 %multmp790)
  store i64 %__mask_count_0_load789, ptr %199, align 8
  store ptr %arr_data791, ptr %200, align 8
  %arr_header792 = call ptr @malloc(i64 24)
  %201 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header792, i32 0, i32 0
  %202 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header792, i32 0, i32 1
  %203 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header792, i32 0, i32 2
  store i64 0, ptr %201, align 8
  %__mask_count_0_load793 = load i64, ptr @__mask_count_0, align 8
  %multmp794 = mul i64 %__mask_count_0_load793, 8
  %arr_data795 = call ptr @malloc(i64 %multmp794)
  store i64 %__mask_count_0_load793, ptr %202, align 8
  store ptr %arr_data795, ptr %203, align 8
  %arr_header796 = call ptr @malloc(i64 24)
  %204 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header796, i32 0, i32 0
  %205 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header796, i32 0, i32 1
  %206 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header796, i32 0, i32 2
  store i64 0, ptr %204, align 8
  %__mask_count_0_load797 = load i64, ptr @__mask_count_0, align 8
  %multmp798 = mul i64 %__mask_count_0_load797, 8
  %arr_data799 = call ptr @malloc(i64 %multmp798)
  store i64 %__mask_count_0_load797, ptr %205, align 8
  store ptr %arr_data799, ptr %206, align 8
  %arr_header800 = call ptr @malloc(i64 24)
  %207 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header800, i32 0, i32 0
  %208 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header800, i32 0, i32 1
  %209 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header800, i32 0, i32 2
  store i64 0, ptr %207, align 8
  %__mask_count_0_load801 = load i64, ptr @__mask_count_0, align 8
  %multmp802 = mul i64 %__mask_count_0_load801, 8
  %arr_data803 = call ptr @malloc(i64 %multmp802)
  store i64 %__mask_count_0_load801, ptr %208, align 8
  store ptr %arr_data803, ptr %209, align 8
  %arr_header804 = call ptr @malloc(i64 24)
  %210 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header804, i32 0, i32 0
  %211 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header804, i32 0, i32 1
  %212 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header804, i32 0, i32 2
  store i64 0, ptr %210, align 8
  %__mask_count_0_load805 = load i64, ptr @__mask_count_0, align 8
  %multmp806 = mul i64 %__mask_count_0_load805, 8
  %arr_data807 = call ptr @malloc(i64 %multmp806)
  store i64 %__mask_count_0_load805, ptr %211, align 8
  store ptr %arr_data807, ptr %212, align 8
  %arr_header808 = call ptr @malloc(i64 24)
  %213 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header808, i32 0, i32 0
  %214 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header808, i32 0, i32 1
  %215 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header808, i32 0, i32 2
  store i64 0, ptr %213, align 8
  %__mask_count_0_load809 = load i64, ptr @__mask_count_0, align 8
  %multmp810 = mul i64 %__mask_count_0_load809, 8
  %arr_data811 = call ptr @malloc(i64 %multmp810)
  store i64 %__mask_count_0_load809, ptr %214, align 8
  store ptr %arr_data811, ptr %215, align 8
  %arr_header812 = call ptr @malloc(i64 24)
  %216 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header812, i32 0, i32 0
  %217 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header812, i32 0, i32 1
  %218 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header812, i32 0, i32 2
  store i64 0, ptr %216, align 8
  %__mask_count_0_load813 = load i64, ptr @__mask_count_0, align 8
  %multmp814 = mul i64 %__mask_count_0_load813, 1
  %arr_data815 = call ptr @malloc(i64 %multmp814)
  store i64 %__mask_count_0_load813, ptr %217, align 8
  store ptr %arr_data815, ptr %218, align 8
  %arr_header816 = call ptr @malloc(i64 24)
  %219 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header816, i32 0, i32 0
  %220 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header816, i32 0, i32 1
  %221 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header816, i32 0, i32 2
  store i64 0, ptr %219, align 8
  %__mask_count_0_load817 = load i64, ptr @__mask_count_0, align 8
  %multmp818 = mul i64 %__mask_count_0_load817, 1
  %arr_data819 = call ptr @malloc(i64 %multmp818)
  store i64 %__mask_count_0_load817, ptr %220, align 8
  store ptr %arr_data819, ptr %221, align 8
  %arr_header820 = call ptr @malloc(i64 24)
  %222 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header820, i32 0, i32 0
  %223 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header820, i32 0, i32 1
  %224 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header820, i32 0, i32 2
  store i64 0, ptr %222, align 8
  %__mask_count_0_load821 = load i64, ptr @__mask_count_0, align 8
  %multmp822 = mul i64 %__mask_count_0_load821, 1
  %arr_data823 = call ptr @malloc(i64 %multmp822)
  store i64 %__mask_count_0_load821, ptr %223, align 8
  store ptr %arr_data823, ptr %224, align 8
  %arr_header824 = call ptr @malloc(i64 24)
  %225 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header824, i32 0, i32 0
  %226 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header824, i32 0, i32 1
  %227 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header824, i32 0, i32 2
  store i64 0, ptr %225, align 8
  %__mask_count_0_load825 = load i64, ptr @__mask_count_0, align 8
  %multmp826 = mul i64 %__mask_count_0_load825, 1
  %arr_data827 = call ptr @malloc(i64 %multmp826)
  store i64 %__mask_count_0_load825, ptr %226, align 8
  store ptr %arr_data827, ptr %227, align 8
  %arr_header828 = call ptr @malloc(i64 24)
  %228 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header828, i32 0, i32 0
  %229 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header828, i32 0, i32 1
  %230 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header828, i32 0, i32 2
  store i64 0, ptr %228, align 8
  %__mask_count_0_load829 = load i64, ptr @__mask_count_0, align 8
  %multmp830 = mul i64 %__mask_count_0_load829, 1
  %arr_data831 = call ptr @malloc(i64 %multmp830)
  store i64 %__mask_count_0_load829, ptr %229, align 8
  store ptr %arr_data831, ptr %230, align 8
  %arr_header832 = call ptr @malloc(i64 24)
  %231 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header832, i32 0, i32 0
  %232 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header832, i32 0, i32 1
  %233 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header832, i32 0, i32 2
  store i64 0, ptr %231, align 8
  %__mask_count_0_load833 = load i64, ptr @__mask_count_0, align 8
  %multmp834 = mul i64 %__mask_count_0_load833, 1
  %arr_data835 = call ptr @malloc(i64 %multmp834)
  store i64 %__mask_count_0_load833, ptr %232, align 8
  store ptr %arr_data835, ptr %233, align 8
  %arr_header836 = call ptr @malloc(i64 24)
  %234 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header836, i32 0, i32 0
  %235 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header836, i32 0, i32 1
  %236 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header836, i32 0, i32 2
  store i64 0, ptr %234, align 8
  %__mask_count_0_load837 = load i64, ptr @__mask_count_0, align 8
  %multmp838 = mul i64 %__mask_count_0_load837, 1
  %arr_data839 = call ptr @malloc(i64 %multmp838)
  store i64 %__mask_count_0_load837, ptr %235, align 8
  store ptr %arr_data839, ptr %236, align 8
  %arr_header840 = call ptr @malloc(i64 24)
  %237 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header840, i32 0, i32 0
  %238 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header840, i32 0, i32 1
  %239 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header840, i32 0, i32 2
  store i64 0, ptr %237, align 8
  %__mask_count_0_load841 = load i64, ptr @__mask_count_0, align 8
  %multmp842 = mul i64 %__mask_count_0_load841, 1
  %arr_data843 = call ptr @malloc(i64 %multmp842)
  store i64 %__mask_count_0_load841, ptr %238, align 8
  store ptr %arr_data843, ptr %239, align 8
  %arr_header844 = call ptr @malloc(i64 24)
  %240 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header844, i32 0, i32 0
  %241 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header844, i32 0, i32 1
  %242 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header844, i32 0, i32 2
  store i64 0, ptr %240, align 8
  %__mask_count_0_load845 = load i64, ptr @__mask_count_0, align 8
  %multmp846 = mul i64 %__mask_count_0_load845, 1
  %arr_data847 = call ptr @malloc(i64 %multmp846)
  store i64 %__mask_count_0_load845, ptr %241, align 8
  store ptr %arr_data847, ptr %242, align 8
  %arr_header848 = call ptr @malloc(i64 24)
  %243 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header848, i32 0, i32 0
  %244 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header848, i32 0, i32 1
  %245 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header848, i32 0, i32 2
  store i64 0, ptr %243, align 8
  %__mask_count_0_load849 = load i64, ptr @__mask_count_0, align 8
  %multmp850 = mul i64 %__mask_count_0_load849, 1
  %arr_data851 = call ptr @malloc(i64 %multmp850)
  store i64 %__mask_count_0_load849, ptr %244, align 8
  store ptr %arr_data851, ptr %245, align 8
  %arr_header852 = call ptr @malloc(i64 24)
  %246 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header852, i32 0, i32 0
  %247 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header852, i32 0, i32 1
  %248 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header852, i32 0, i32 2
  store i64 0, ptr %246, align 8
  %__mask_count_0_load853 = load i64, ptr @__mask_count_0, align 8
  %multmp854 = mul i64 %__mask_count_0_load853, 1
  %arr_data855 = call ptr @malloc(i64 %multmp854)
  store i64 %__mask_count_0_load853, ptr %247, align 8
  store ptr %arr_data855, ptr %248, align 8
  %arr_header856 = call ptr @malloc(i64 24)
  %249 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header856, i32 0, i32 0
  %250 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header856, i32 0, i32 1
  %251 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header856, i32 0, i32 2
  store i64 0, ptr %249, align 8
  %__mask_count_0_load857 = load i64, ptr @__mask_count_0, align 8
  %multmp858 = mul i64 %__mask_count_0_load857, 1
  %arr_data859 = call ptr @malloc(i64 %multmp858)
  store i64 %__mask_count_0_load857, ptr %250, align 8
  store ptr %arr_data859, ptr %251, align 8
  %arr_header860 = call ptr @malloc(i64 24)
  %252 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header860, i32 0, i32 0
  %253 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header860, i32 0, i32 1
  %254 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header860, i32 0, i32 2
  store i64 0, ptr %252, align 8
  %__mask_count_0_load861 = load i64, ptr @__mask_count_0, align 8
  %multmp862 = mul i64 %__mask_count_0_load861, 1
  %arr_data863 = call ptr @malloc(i64 %multmp862)
  store i64 %__mask_count_0_load861, ptr %253, align 8
  store ptr %arr_data863, ptr %254, align 8
  %arr_header864 = call ptr @malloc(i64 24)
  %255 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header864, i32 0, i32 0
  %256 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header864, i32 0, i32 1
  %257 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header864, i32 0, i32 2
  store i64 0, ptr %255, align 8
  %__mask_count_0_load865 = load i64, ptr @__mask_count_0, align 8
  %multmp866 = mul i64 %__mask_count_0_load865, 1
  %arr_data867 = call ptr @malloc(i64 %multmp866)
  store i64 %__mask_count_0_load865, ptr %256, align 8
  store ptr %arr_data867, ptr %257, align 8
  %arr_header868 = call ptr @malloc(i64 24)
  %258 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header868, i32 0, i32 0
  %259 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header868, i32 0, i32 1
  %260 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header868, i32 0, i32 2
  store i64 0, ptr %258, align 8
  %__mask_count_0_load869 = load i64, ptr @__mask_count_0, align 8
  %multmp870 = mul i64 %__mask_count_0_load869, 1
  %arr_data871 = call ptr @malloc(i64 %multmp870)
  store i64 %__mask_count_0_load869, ptr %259, align 8
  store ptr %arr_data871, ptr %260, align 8
  %arr_header872 = call ptr @malloc(i64 24)
  %261 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header872, i32 0, i32 0
  %262 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header872, i32 0, i32 1
  %263 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header872, i32 0, i32 2
  store i64 0, ptr %261, align 8
  %__mask_count_0_load873 = load i64, ptr @__mask_count_0, align 8
  %multmp874 = mul i64 %__mask_count_0_load873, 1
  %arr_data875 = call ptr @malloc(i64 %multmp874)
  store i64 %__mask_count_0_load873, ptr %262, align 8
  store ptr %arr_data875, ptr %263, align 8
  %data_header876 = call ptr @malloc(i64 32)
  %data_buffer = call ptr @malloc(i64 296)
  %264 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header876, i32 0, i32 0
  store i64 37, ptr %264, align 4
  %265 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header876, i32 0, i32 1
  store i64 37, ptr %265, align 4
  %266 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header876, i32 0, i32 2
  store ptr %data_buffer, ptr %266, align 8
  %data_gep = getelementptr ptr, ptr %data_buffer, i64 0
  store ptr %arr_header730, ptr %data_gep, align 8
  %data_gep877 = getelementptr ptr, ptr %data_buffer, i64 1
  store ptr %arr_header732, ptr %data_gep877, align 8
  %data_gep878 = getelementptr ptr, ptr %data_buffer, i64 2
  store ptr %arr_header736, ptr %data_gep878, align 8
  %data_gep879 = getelementptr ptr, ptr %data_buffer, i64 3
  store ptr %arr_header740, ptr %data_gep879, align 8
  %data_gep880 = getelementptr ptr, ptr %data_buffer, i64 4
  store ptr %arr_header744, ptr %data_gep880, align 8
  %data_gep881 = getelementptr ptr, ptr %data_buffer, i64 5
  store ptr %arr_header748, ptr %data_gep881, align 8
  %data_gep882 = getelementptr ptr, ptr %data_buffer, i64 6
  store ptr %arr_header752, ptr %data_gep882, align 8
  %data_gep883 = getelementptr ptr, ptr %data_buffer, i64 7
  store ptr %arr_header756, ptr %data_gep883, align 8
  %data_gep884 = getelementptr ptr, ptr %data_buffer, i64 8
  store ptr %arr_header760, ptr %data_gep884, align 8
  %data_gep885 = getelementptr ptr, ptr %data_buffer, i64 9
  store ptr %arr_header764, ptr %data_gep885, align 8
  %data_gep886 = getelementptr ptr, ptr %data_buffer, i64 10
  store ptr %arr_header768, ptr %data_gep886, align 8
  %data_gep887 = getelementptr ptr, ptr %data_buffer, i64 11
  store ptr %arr_header772, ptr %data_gep887, align 8
  %data_gep888 = getelementptr ptr, ptr %data_buffer, i64 12
  store ptr %arr_header776, ptr %data_gep888, align 8
  %data_gep889 = getelementptr ptr, ptr %data_buffer, i64 13
  store ptr %arr_header780, ptr %data_gep889, align 8
  %data_gep890 = getelementptr ptr, ptr %data_buffer, i64 14
  store ptr %arr_header784, ptr %data_gep890, align 8
  %data_gep891 = getelementptr ptr, ptr %data_buffer, i64 15
  store ptr %arr_header788, ptr %data_gep891, align 8
  %data_gep892 = getelementptr ptr, ptr %data_buffer, i64 16
  store ptr %arr_header792, ptr %data_gep892, align 8
  %data_gep893 = getelementptr ptr, ptr %data_buffer, i64 17
  store ptr %arr_header796, ptr %data_gep893, align 8
  %data_gep894 = getelementptr ptr, ptr %data_buffer, i64 18
  store ptr %arr_header800, ptr %data_gep894, align 8
  %data_gep895 = getelementptr ptr, ptr %data_buffer, i64 19
  store ptr %arr_header804, ptr %data_gep895, align 8
  %data_gep896 = getelementptr ptr, ptr %data_buffer, i64 20
  store ptr %arr_header808, ptr %data_gep896, align 8
  %data_gep897 = getelementptr ptr, ptr %data_buffer, i64 21
  store ptr %arr_header812, ptr %data_gep897, align 8
  %data_gep898 = getelementptr ptr, ptr %data_buffer, i64 22
  store ptr %arr_header816, ptr %data_gep898, align 8
  %data_gep899 = getelementptr ptr, ptr %data_buffer, i64 23
  store ptr %arr_header820, ptr %data_gep899, align 8
  %data_gep900 = getelementptr ptr, ptr %data_buffer, i64 24
  store ptr %arr_header824, ptr %data_gep900, align 8
  %data_gep901 = getelementptr ptr, ptr %data_buffer, i64 25
  store ptr %arr_header828, ptr %data_gep901, align 8
  %data_gep902 = getelementptr ptr, ptr %data_buffer, i64 26
  store ptr %arr_header832, ptr %data_gep902, align 8
  %data_gep903 = getelementptr ptr, ptr %data_buffer, i64 27
  store ptr %arr_header836, ptr %data_gep903, align 8
  %data_gep904 = getelementptr ptr, ptr %data_buffer, i64 28
  store ptr %arr_header840, ptr %data_gep904, align 8
  %data_gep905 = getelementptr ptr, ptr %data_buffer, i64 29
  store ptr %arr_header844, ptr %data_gep905, align 8
  %data_gep906 = getelementptr ptr, ptr %data_buffer, i64 30
  store ptr %arr_header848, ptr %data_gep906, align 8
  %data_gep907 = getelementptr ptr, ptr %data_buffer, i64 31
  store ptr %arr_header852, ptr %data_gep907, align 8
  %data_gep908 = getelementptr ptr, ptr %data_buffer, i64 32
  store ptr %arr_header856, ptr %data_gep908, align 8
  %data_gep909 = getelementptr ptr, ptr %data_buffer, i64 33
  store ptr %arr_header860, ptr %data_gep909, align 8
  %data_gep910 = getelementptr ptr, ptr %data_buffer, i64 34
  store ptr %arr_header864, ptr %data_gep910, align 8
  %data_gep911 = getelementptr ptr, ptr %data_buffer, i64 35
  store ptr %arr_header868, ptr %data_gep911, align 8
  %data_gep912 = getelementptr ptr, ptr %data_buffer, i64 36
  store ptr %arr_header872, ptr %data_gep912, align 8
  %arr_header913 = call ptr @malloc(i64 24)
  %267 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header913, i32 0, i32 0
  %268 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header913, i32 0, i32 1
  %269 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header913, i32 0, i32 2
  store i64 37, ptr %267, align 8
  %arr_data914 = call ptr @malloc(i64 296)
  store i64 37, ptr %268, align 8
  store ptr %arr_data914, ptr %269, align 8
  %elem_ptr915 = getelementptr ptr, ptr %arr_data914, i64 0
  store ptr @str, ptr %elem_ptr915, align 8
  %elem_ptr916 = getelementptr ptr, ptr %arr_data914, i64 1
  store ptr @str.39, ptr %elem_ptr916, align 8
  %elem_ptr917 = getelementptr ptr, ptr %arr_data914, i64 2
  store ptr @str.40, ptr %elem_ptr917, align 8
  %elem_ptr918 = getelementptr ptr, ptr %arr_data914, i64 3
  store ptr @str.41, ptr %elem_ptr918, align 8
  %elem_ptr919 = getelementptr ptr, ptr %arr_data914, i64 4
  store ptr @str.42, ptr %elem_ptr919, align 8
  %elem_ptr920 = getelementptr ptr, ptr %arr_data914, i64 5
  store ptr @str.43, ptr %elem_ptr920, align 8
  %elem_ptr921 = getelementptr ptr, ptr %arr_data914, i64 6
  store ptr @str.44, ptr %elem_ptr921, align 8
  %elem_ptr922 = getelementptr ptr, ptr %arr_data914, i64 7
  store ptr @str.45, ptr %elem_ptr922, align 8
  %elem_ptr923 = getelementptr ptr, ptr %arr_data914, i64 8
  store ptr @str.46, ptr %elem_ptr923, align 8
  %elem_ptr924 = getelementptr ptr, ptr %arr_data914, i64 9
  store ptr @str.47, ptr %elem_ptr924, align 8
  %elem_ptr925 = getelementptr ptr, ptr %arr_data914, i64 10
  store ptr @str.48, ptr %elem_ptr925, align 8
  %elem_ptr926 = getelementptr ptr, ptr %arr_data914, i64 11
  store ptr @str.49, ptr %elem_ptr926, align 8
  %elem_ptr927 = getelementptr ptr, ptr %arr_data914, i64 12
  store ptr @str.50, ptr %elem_ptr927, align 8
  %elem_ptr928 = getelementptr ptr, ptr %arr_data914, i64 13
  store ptr @str.51, ptr %elem_ptr928, align 8
  %elem_ptr929 = getelementptr ptr, ptr %arr_data914, i64 14
  store ptr @str.52, ptr %elem_ptr929, align 8
  %elem_ptr930 = getelementptr ptr, ptr %arr_data914, i64 15
  store ptr @str.53, ptr %elem_ptr930, align 8
  %elem_ptr931 = getelementptr ptr, ptr %arr_data914, i64 16
  store ptr @str.54, ptr %elem_ptr931, align 8
  %elem_ptr932 = getelementptr ptr, ptr %arr_data914, i64 17
  store ptr @str.55, ptr %elem_ptr932, align 8
  %elem_ptr933 = getelementptr ptr, ptr %arr_data914, i64 18
  store ptr @str.56, ptr %elem_ptr933, align 8
  %elem_ptr934 = getelementptr ptr, ptr %arr_data914, i64 19
  store ptr @str.57, ptr %elem_ptr934, align 8
  %elem_ptr935 = getelementptr ptr, ptr %arr_data914, i64 20
  store ptr @str.58, ptr %elem_ptr935, align 8
  %elem_ptr936 = getelementptr ptr, ptr %arr_data914, i64 21
  store ptr @str.59, ptr %elem_ptr936, align 8
  %elem_ptr937 = getelementptr ptr, ptr %arr_data914, i64 22
  store ptr @str.60, ptr %elem_ptr937, align 8
  %elem_ptr938 = getelementptr ptr, ptr %arr_data914, i64 23
  store ptr @str.61, ptr %elem_ptr938, align 8
  %elem_ptr939 = getelementptr ptr, ptr %arr_data914, i64 24
  store ptr @str.62, ptr %elem_ptr939, align 8
  %elem_ptr940 = getelementptr ptr, ptr %arr_data914, i64 25
  store ptr @str.63, ptr %elem_ptr940, align 8
  %elem_ptr941 = getelementptr ptr, ptr %arr_data914, i64 26
  store ptr @str.64, ptr %elem_ptr941, align 8
  %elem_ptr942 = getelementptr ptr, ptr %arr_data914, i64 27
  store ptr @str.65, ptr %elem_ptr942, align 8
  %elem_ptr943 = getelementptr ptr, ptr %arr_data914, i64 28
  store ptr @str.66, ptr %elem_ptr943, align 8
  %elem_ptr944 = getelementptr ptr, ptr %arr_data914, i64 29
  store ptr @str.67, ptr %elem_ptr944, align 8
  %elem_ptr945 = getelementptr ptr, ptr %arr_data914, i64 30
  store ptr @str.68, ptr %elem_ptr945, align 8
  %elem_ptr946 = getelementptr ptr, ptr %arr_data914, i64 31
  store ptr @str.69, ptr %elem_ptr946, align 8
  %elem_ptr947 = getelementptr ptr, ptr %arr_data914, i64 32
  store ptr @str.70, ptr %elem_ptr947, align 8
  %elem_ptr948 = getelementptr ptr, ptr %arr_data914, i64 33
  store ptr @str.71, ptr %elem_ptr948, align 8
  %elem_ptr949 = getelementptr ptr, ptr %arr_data914, i64 34
  store ptr @str.72, ptr %elem_ptr949, align 8
  %elem_ptr950 = getelementptr ptr, ptr %arr_data914, i64 35
  store ptr @str.73, ptr %elem_ptr950, align 8
  %elem_ptr951 = getelementptr ptr, ptr %arr_data914, i64 36
  store ptr @str.74, ptr %elem_ptr951, align 8
  %arr_header952 = call ptr @malloc(i64 24)
  %270 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header952, i32 0, i32 0
  %271 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header952, i32 0, i32 1
  %272 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header952, i32 0, i32 2
  store i64 37, ptr %270, align 8
  %arr_data953 = call ptr @malloc(i64 296)
  store i64 37, ptr %271, align 8
  store ptr %arr_data953, ptr %272, align 8
  %elem_ptr954 = getelementptr i64, ptr %arr_data953, i64 0
  store i64 4, ptr %elem_ptr954, align 8
  %elem_ptr955 = getelementptr i64, ptr %arr_data953, i64 1
  store i64 2, ptr %elem_ptr955, align 8
  %elem_ptr956 = getelementptr i64, ptr %arr_data953, i64 2
  store i64 2, ptr %elem_ptr956, align 8
  %elem_ptr957 = getelementptr i64, ptr %arr_data953, i64 3
  store i64 2, ptr %elem_ptr957, align 8
  %elem_ptr958 = getelementptr i64, ptr %arr_data953, i64 4
  store i64 2, ptr %elem_ptr958, align 8
  %elem_ptr959 = getelementptr i64, ptr %arr_data953, i64 5
  store i64 2, ptr %elem_ptr959, align 8
  %elem_ptr960 = getelementptr i64, ptr %arr_data953, i64 6
  store i64 2, ptr %elem_ptr960, align 8
  %elem_ptr961 = getelementptr i64, ptr %arr_data953, i64 7
  store i64 2, ptr %elem_ptr961, align 8
  %elem_ptr962 = getelementptr i64, ptr %arr_data953, i64 8
  store i64 2, ptr %elem_ptr962, align 8
  %elem_ptr963 = getelementptr i64, ptr %arr_data953, i64 9
  store i64 2, ptr %elem_ptr963, align 8
  %elem_ptr964 = getelementptr i64, ptr %arr_data953, i64 10
  store i64 2, ptr %elem_ptr964, align 8
  %elem_ptr965 = getelementptr i64, ptr %arr_data953, i64 11
  store i64 2, ptr %elem_ptr965, align 8
  %elem_ptr966 = getelementptr i64, ptr %arr_data953, i64 12
  store i64 2, ptr %elem_ptr966, align 8
  %elem_ptr967 = getelementptr i64, ptr %arr_data953, i64 13
  store i64 2, ptr %elem_ptr967, align 8
  %elem_ptr968 = getelementptr i64, ptr %arr_data953, i64 14
  store i64 2, ptr %elem_ptr968, align 8
  %elem_ptr969 = getelementptr i64, ptr %arr_data953, i64 15
  store i64 2, ptr %elem_ptr969, align 8
  %elem_ptr970 = getelementptr i64, ptr %arr_data953, i64 16
  store i64 2, ptr %elem_ptr970, align 8
  %elem_ptr971 = getelementptr i64, ptr %arr_data953, i64 17
  store i64 2, ptr %elem_ptr971, align 8
  %elem_ptr972 = getelementptr i64, ptr %arr_data953, i64 18
  store i64 2, ptr %elem_ptr972, align 8
  %elem_ptr973 = getelementptr i64, ptr %arr_data953, i64 19
  store i64 2, ptr %elem_ptr973, align 8
  %elem_ptr974 = getelementptr i64, ptr %arr_data953, i64 20
  store i64 1, ptr %elem_ptr974, align 8
  %elem_ptr975 = getelementptr i64, ptr %arr_data953, i64 21
  store i64 3, ptr %elem_ptr975, align 8
  %elem_ptr976 = getelementptr i64, ptr %arr_data953, i64 22
  store i64 3, ptr %elem_ptr976, align 8
  %elem_ptr977 = getelementptr i64, ptr %arr_data953, i64 23
  store i64 3, ptr %elem_ptr977, align 8
  %elem_ptr978 = getelementptr i64, ptr %arr_data953, i64 24
  store i64 3, ptr %elem_ptr978, align 8
  %elem_ptr979 = getelementptr i64, ptr %arr_data953, i64 25
  store i64 3, ptr %elem_ptr979, align 8
  %elem_ptr980 = getelementptr i64, ptr %arr_data953, i64 26
  store i64 3, ptr %elem_ptr980, align 8
  %elem_ptr981 = getelementptr i64, ptr %arr_data953, i64 27
  store i64 3, ptr %elem_ptr981, align 8
  %elem_ptr982 = getelementptr i64, ptr %arr_data953, i64 28
  store i64 3, ptr %elem_ptr982, align 8
  %elem_ptr983 = getelementptr i64, ptr %arr_data953, i64 29
  store i64 3, ptr %elem_ptr983, align 8
  %elem_ptr984 = getelementptr i64, ptr %arr_data953, i64 30
  store i64 3, ptr %elem_ptr984, align 8
  %elem_ptr985 = getelementptr i64, ptr %arr_data953, i64 31
  store i64 3, ptr %elem_ptr985, align 8
  %elem_ptr986 = getelementptr i64, ptr %arr_data953, i64 32
  store i64 3, ptr %elem_ptr986, align 8
  %elem_ptr987 = getelementptr i64, ptr %arr_data953, i64 33
  store i64 3, ptr %elem_ptr987, align 8
  %elem_ptr988 = getelementptr i64, ptr %arr_data953, i64 34
  store i64 3, ptr %elem_ptr988, align 8
  %elem_ptr989 = getelementptr i64, ptr %arr_data953, i64 35
  store i64 3, ptr %elem_ptr989, align 8
  %elem_ptr990 = getelementptr i64, ptr %arr_data953, i64 36
  store i64 3, ptr %elem_ptr990, align 8
  %df_instance = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %273 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 0
  store ptr %arr_header913, ptr %273, align 8
  %274 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 1
  store ptr %data_header876, ptr %274, align 8
  %275 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 2
  store ptr %arr_header952, ptr %275, align 8
  %276 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 3
  store i64 0, ptr %276, align 4
  store ptr %df_instance, ptr @__where_result_0, align 8
  %__where_result_0_load = load ptr, ptr @__where_result_0, align 8
  %__mask_count_0_load991 = load i64, ptr @__mask_count_0, align 8
  %rowCount_ptr992 = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %__where_result_0_load, i32 0, i32 3
  store i64 %__mask_count_0_load991, ptr %rowCount_ptr992, align 8
  %__where_result_0_load993 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr994 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load993, i32 0, i32 1
  %data_header995 = load ptr, ptr %df_data_ptr994, align 8
  %277 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header995, i32 0, i32 2
  %278 = load ptr, ptr %277, align 8
  %279 = getelementptr ptr, ptr %278, i64 0
  %280 = load ptr, ptr %279, align 8
  %__mask_count_0_load996 = load i64, ptr @__mask_count_0, align 8
  %len_ptr997 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %280, i32 0, i32 0
  store i64 %__mask_count_0_load996, ptr %len_ptr997, align 8
  %__where_result_0_load998 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr999 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load998, i32 0, i32 1
  %data_header1000 = load ptr, ptr %df_data_ptr999, align 8
  %281 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1000, i32 0, i32 2
  %282 = load ptr, ptr %281, align 8
  %283 = getelementptr ptr, ptr %282, i64 1
  %284 = load ptr, ptr %283, align 8
  %__mask_count_0_load1001 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1002 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %284, i32 0, i32 0
  store i64 %__mask_count_0_load1001, ptr %len_ptr1002, align 8
  %__where_result_0_load1003 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1004 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1003, i32 0, i32 1
  %data_header1005 = load ptr, ptr %df_data_ptr1004, align 8
  %285 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1005, i32 0, i32 2
  %286 = load ptr, ptr %285, align 8
  %287 = getelementptr ptr, ptr %286, i64 2
  %288 = load ptr, ptr %287, align 8
  %__mask_count_0_load1006 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1007 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %288, i32 0, i32 0
  store i64 %__mask_count_0_load1006, ptr %len_ptr1007, align 8
  %__where_result_0_load1008 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1009 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1008, i32 0, i32 1
  %data_header1010 = load ptr, ptr %df_data_ptr1009, align 8
  %289 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1010, i32 0, i32 2
  %290 = load ptr, ptr %289, align 8
  %291 = getelementptr ptr, ptr %290, i64 3
  %292 = load ptr, ptr %291, align 8
  %__mask_count_0_load1011 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1012 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %292, i32 0, i32 0
  store i64 %__mask_count_0_load1011, ptr %len_ptr1012, align 8
  %__where_result_0_load1013 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1014 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1013, i32 0, i32 1
  %data_header1015 = load ptr, ptr %df_data_ptr1014, align 8
  %293 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1015, i32 0, i32 2
  %294 = load ptr, ptr %293, align 8
  %295 = getelementptr ptr, ptr %294, i64 4
  %296 = load ptr, ptr %295, align 8
  %__mask_count_0_load1016 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1017 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %296, i32 0, i32 0
  store i64 %__mask_count_0_load1016, ptr %len_ptr1017, align 8
  %__where_result_0_load1018 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1019 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1018, i32 0, i32 1
  %data_header1020 = load ptr, ptr %df_data_ptr1019, align 8
  %297 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1020, i32 0, i32 2
  %298 = load ptr, ptr %297, align 8
  %299 = getelementptr ptr, ptr %298, i64 5
  %300 = load ptr, ptr %299, align 8
  %__mask_count_0_load1021 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1022 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %300, i32 0, i32 0
  store i64 %__mask_count_0_load1021, ptr %len_ptr1022, align 8
  %__where_result_0_load1023 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1024 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1023, i32 0, i32 1
  %data_header1025 = load ptr, ptr %df_data_ptr1024, align 8
  %301 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1025, i32 0, i32 2
  %302 = load ptr, ptr %301, align 8
  %303 = getelementptr ptr, ptr %302, i64 6
  %304 = load ptr, ptr %303, align 8
  %__mask_count_0_load1026 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1027 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %304, i32 0, i32 0
  store i64 %__mask_count_0_load1026, ptr %len_ptr1027, align 8
  %__where_result_0_load1028 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1029 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1028, i32 0, i32 1
  %data_header1030 = load ptr, ptr %df_data_ptr1029, align 8
  %305 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1030, i32 0, i32 2
  %306 = load ptr, ptr %305, align 8
  %307 = getelementptr ptr, ptr %306, i64 7
  %308 = load ptr, ptr %307, align 8
  %__mask_count_0_load1031 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1032 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %308, i32 0, i32 0
  store i64 %__mask_count_0_load1031, ptr %len_ptr1032, align 8
  %__where_result_0_load1033 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1034 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1033, i32 0, i32 1
  %data_header1035 = load ptr, ptr %df_data_ptr1034, align 8
  %309 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1035, i32 0, i32 2
  %310 = load ptr, ptr %309, align 8
  %311 = getelementptr ptr, ptr %310, i64 8
  %312 = load ptr, ptr %311, align 8
  %__mask_count_0_load1036 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1037 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %312, i32 0, i32 0
  store i64 %__mask_count_0_load1036, ptr %len_ptr1037, align 8
  %__where_result_0_load1038 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1039 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1038, i32 0, i32 1
  %data_header1040 = load ptr, ptr %df_data_ptr1039, align 8
  %313 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1040, i32 0, i32 2
  %314 = load ptr, ptr %313, align 8
  %315 = getelementptr ptr, ptr %314, i64 9
  %316 = load ptr, ptr %315, align 8
  %__mask_count_0_load1041 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1042 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %316, i32 0, i32 0
  store i64 %__mask_count_0_load1041, ptr %len_ptr1042, align 8
  %__where_result_0_load1043 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1044 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1043, i32 0, i32 1
  %data_header1045 = load ptr, ptr %df_data_ptr1044, align 8
  %317 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1045, i32 0, i32 2
  %318 = load ptr, ptr %317, align 8
  %319 = getelementptr ptr, ptr %318, i64 10
  %320 = load ptr, ptr %319, align 8
  %__mask_count_0_load1046 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1047 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %320, i32 0, i32 0
  store i64 %__mask_count_0_load1046, ptr %len_ptr1047, align 8
  %__where_result_0_load1048 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1049 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1048, i32 0, i32 1
  %data_header1050 = load ptr, ptr %df_data_ptr1049, align 8
  %321 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1050, i32 0, i32 2
  %322 = load ptr, ptr %321, align 8
  %323 = getelementptr ptr, ptr %322, i64 11
  %324 = load ptr, ptr %323, align 8
  %__mask_count_0_load1051 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1052 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %324, i32 0, i32 0
  store i64 %__mask_count_0_load1051, ptr %len_ptr1052, align 8
  %__where_result_0_load1053 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1054 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1053, i32 0, i32 1
  %data_header1055 = load ptr, ptr %df_data_ptr1054, align 8
  %325 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1055, i32 0, i32 2
  %326 = load ptr, ptr %325, align 8
  %327 = getelementptr ptr, ptr %326, i64 12
  %328 = load ptr, ptr %327, align 8
  %__mask_count_0_load1056 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1057 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %328, i32 0, i32 0
  store i64 %__mask_count_0_load1056, ptr %len_ptr1057, align 8
  %__where_result_0_load1058 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1059 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1058, i32 0, i32 1
  %data_header1060 = load ptr, ptr %df_data_ptr1059, align 8
  %329 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1060, i32 0, i32 2
  %330 = load ptr, ptr %329, align 8
  %331 = getelementptr ptr, ptr %330, i64 13
  %332 = load ptr, ptr %331, align 8
  %__mask_count_0_load1061 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1062 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %332, i32 0, i32 0
  store i64 %__mask_count_0_load1061, ptr %len_ptr1062, align 8
  %__where_result_0_load1063 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1064 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1063, i32 0, i32 1
  %data_header1065 = load ptr, ptr %df_data_ptr1064, align 8
  %333 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1065, i32 0, i32 2
  %334 = load ptr, ptr %333, align 8
  %335 = getelementptr ptr, ptr %334, i64 14
  %336 = load ptr, ptr %335, align 8
  %__mask_count_0_load1066 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1067 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %336, i32 0, i32 0
  store i64 %__mask_count_0_load1066, ptr %len_ptr1067, align 8
  %__where_result_0_load1068 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1069 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1068, i32 0, i32 1
  %data_header1070 = load ptr, ptr %df_data_ptr1069, align 8
  %337 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1070, i32 0, i32 2
  %338 = load ptr, ptr %337, align 8
  %339 = getelementptr ptr, ptr %338, i64 15
  %340 = load ptr, ptr %339, align 8
  %__mask_count_0_load1071 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1072 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %340, i32 0, i32 0
  store i64 %__mask_count_0_load1071, ptr %len_ptr1072, align 8
  %__where_result_0_load1073 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1074 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1073, i32 0, i32 1
  %data_header1075 = load ptr, ptr %df_data_ptr1074, align 8
  %341 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1075, i32 0, i32 2
  %342 = load ptr, ptr %341, align 8
  %343 = getelementptr ptr, ptr %342, i64 16
  %344 = load ptr, ptr %343, align 8
  %__mask_count_0_load1076 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1077 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %344, i32 0, i32 0
  store i64 %__mask_count_0_load1076, ptr %len_ptr1077, align 8
  %__where_result_0_load1078 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1079 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1078, i32 0, i32 1
  %data_header1080 = load ptr, ptr %df_data_ptr1079, align 8
  %345 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1080, i32 0, i32 2
  %346 = load ptr, ptr %345, align 8
  %347 = getelementptr ptr, ptr %346, i64 17
  %348 = load ptr, ptr %347, align 8
  %__mask_count_0_load1081 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1082 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %348, i32 0, i32 0
  store i64 %__mask_count_0_load1081, ptr %len_ptr1082, align 8
  %__where_result_0_load1083 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1084 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1083, i32 0, i32 1
  %data_header1085 = load ptr, ptr %df_data_ptr1084, align 8
  %349 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1085, i32 0, i32 2
  %350 = load ptr, ptr %349, align 8
  %351 = getelementptr ptr, ptr %350, i64 18
  %352 = load ptr, ptr %351, align 8
  %__mask_count_0_load1086 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1087 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %352, i32 0, i32 0
  store i64 %__mask_count_0_load1086, ptr %len_ptr1087, align 8
  %__where_result_0_load1088 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1089 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1088, i32 0, i32 1
  %data_header1090 = load ptr, ptr %df_data_ptr1089, align 8
  %353 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1090, i32 0, i32 2
  %354 = load ptr, ptr %353, align 8
  %355 = getelementptr ptr, ptr %354, i64 19
  %356 = load ptr, ptr %355, align 8
  %__mask_count_0_load1091 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1092 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %356, i32 0, i32 0
  store i64 %__mask_count_0_load1091, ptr %len_ptr1092, align 8
  %__where_result_0_load1093 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1094 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1093, i32 0, i32 1
  %data_header1095 = load ptr, ptr %df_data_ptr1094, align 8
  %357 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1095, i32 0, i32 2
  %358 = load ptr, ptr %357, align 8
  %359 = getelementptr ptr, ptr %358, i64 20
  %360 = load ptr, ptr %359, align 8
  %__mask_count_0_load1096 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1097 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %360, i32 0, i32 0
  store i64 %__mask_count_0_load1096, ptr %len_ptr1097, align 8
  %__where_result_0_load1098 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1099 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1098, i32 0, i32 1
  %data_header1100 = load ptr, ptr %df_data_ptr1099, align 8
  %361 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1100, i32 0, i32 2
  %362 = load ptr, ptr %361, align 8
  %363 = getelementptr ptr, ptr %362, i64 21
  %364 = load ptr, ptr %363, align 8
  %__mask_count_0_load1101 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1102 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %364, i32 0, i32 0
  store i64 %__mask_count_0_load1101, ptr %len_ptr1102, align 8
  %__where_result_0_load1103 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1104 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1103, i32 0, i32 1
  %data_header1105 = load ptr, ptr %df_data_ptr1104, align 8
  %365 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1105, i32 0, i32 2
  %366 = load ptr, ptr %365, align 8
  %367 = getelementptr ptr, ptr %366, i64 22
  %368 = load ptr, ptr %367, align 8
  %__mask_count_0_load1106 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1107 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %368, i32 0, i32 0
  store i64 %__mask_count_0_load1106, ptr %len_ptr1107, align 8
  %__where_result_0_load1108 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1109 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1108, i32 0, i32 1
  %data_header1110 = load ptr, ptr %df_data_ptr1109, align 8
  %369 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1110, i32 0, i32 2
  %370 = load ptr, ptr %369, align 8
  %371 = getelementptr ptr, ptr %370, i64 23
  %372 = load ptr, ptr %371, align 8
  %__mask_count_0_load1111 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1112 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %372, i32 0, i32 0
  store i64 %__mask_count_0_load1111, ptr %len_ptr1112, align 8
  %__where_result_0_load1113 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1114 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1113, i32 0, i32 1
  %data_header1115 = load ptr, ptr %df_data_ptr1114, align 8
  %373 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1115, i32 0, i32 2
  %374 = load ptr, ptr %373, align 8
  %375 = getelementptr ptr, ptr %374, i64 24
  %376 = load ptr, ptr %375, align 8
  %__mask_count_0_load1116 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1117 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %376, i32 0, i32 0
  store i64 %__mask_count_0_load1116, ptr %len_ptr1117, align 8
  %__where_result_0_load1118 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1119 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1118, i32 0, i32 1
  %data_header1120 = load ptr, ptr %df_data_ptr1119, align 8
  %377 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1120, i32 0, i32 2
  %378 = load ptr, ptr %377, align 8
  %379 = getelementptr ptr, ptr %378, i64 25
  %380 = load ptr, ptr %379, align 8
  %__mask_count_0_load1121 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1122 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %380, i32 0, i32 0
  store i64 %__mask_count_0_load1121, ptr %len_ptr1122, align 8
  %__where_result_0_load1123 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1124 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1123, i32 0, i32 1
  %data_header1125 = load ptr, ptr %df_data_ptr1124, align 8
  %381 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1125, i32 0, i32 2
  %382 = load ptr, ptr %381, align 8
  %383 = getelementptr ptr, ptr %382, i64 26
  %384 = load ptr, ptr %383, align 8
  %__mask_count_0_load1126 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1127 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %384, i32 0, i32 0
  store i64 %__mask_count_0_load1126, ptr %len_ptr1127, align 8
  %__where_result_0_load1128 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1129 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1128, i32 0, i32 1
  %data_header1130 = load ptr, ptr %df_data_ptr1129, align 8
  %385 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1130, i32 0, i32 2
  %386 = load ptr, ptr %385, align 8
  %387 = getelementptr ptr, ptr %386, i64 27
  %388 = load ptr, ptr %387, align 8
  %__mask_count_0_load1131 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1132 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %388, i32 0, i32 0
  store i64 %__mask_count_0_load1131, ptr %len_ptr1132, align 8
  %__where_result_0_load1133 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1134 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1133, i32 0, i32 1
  %data_header1135 = load ptr, ptr %df_data_ptr1134, align 8
  %389 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1135, i32 0, i32 2
  %390 = load ptr, ptr %389, align 8
  %391 = getelementptr ptr, ptr %390, i64 28
  %392 = load ptr, ptr %391, align 8
  %__mask_count_0_load1136 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1137 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %392, i32 0, i32 0
  store i64 %__mask_count_0_load1136, ptr %len_ptr1137, align 8
  %__where_result_0_load1138 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1139 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1138, i32 0, i32 1
  %data_header1140 = load ptr, ptr %df_data_ptr1139, align 8
  %393 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1140, i32 0, i32 2
  %394 = load ptr, ptr %393, align 8
  %395 = getelementptr ptr, ptr %394, i64 29
  %396 = load ptr, ptr %395, align 8
  %__mask_count_0_load1141 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1142 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %396, i32 0, i32 0
  store i64 %__mask_count_0_load1141, ptr %len_ptr1142, align 8
  %__where_result_0_load1143 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1144 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1143, i32 0, i32 1
  %data_header1145 = load ptr, ptr %df_data_ptr1144, align 8
  %397 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1145, i32 0, i32 2
  %398 = load ptr, ptr %397, align 8
  %399 = getelementptr ptr, ptr %398, i64 30
  %400 = load ptr, ptr %399, align 8
  %__mask_count_0_load1146 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1147 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %400, i32 0, i32 0
  store i64 %__mask_count_0_load1146, ptr %len_ptr1147, align 8
  %__where_result_0_load1148 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1149 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1148, i32 0, i32 1
  %data_header1150 = load ptr, ptr %df_data_ptr1149, align 8
  %401 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1150, i32 0, i32 2
  %402 = load ptr, ptr %401, align 8
  %403 = getelementptr ptr, ptr %402, i64 31
  %404 = load ptr, ptr %403, align 8
  %__mask_count_0_load1151 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1152 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %404, i32 0, i32 0
  store i64 %__mask_count_0_load1151, ptr %len_ptr1152, align 8
  %__where_result_0_load1153 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1154 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1153, i32 0, i32 1
  %data_header1155 = load ptr, ptr %df_data_ptr1154, align 8
  %405 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1155, i32 0, i32 2
  %406 = load ptr, ptr %405, align 8
  %407 = getelementptr ptr, ptr %406, i64 32
  %408 = load ptr, ptr %407, align 8
  %__mask_count_0_load1156 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1157 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %408, i32 0, i32 0
  store i64 %__mask_count_0_load1156, ptr %len_ptr1157, align 8
  %__where_result_0_load1158 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1159 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1158, i32 0, i32 1
  %data_header1160 = load ptr, ptr %df_data_ptr1159, align 8
  %409 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1160, i32 0, i32 2
  %410 = load ptr, ptr %409, align 8
  %411 = getelementptr ptr, ptr %410, i64 33
  %412 = load ptr, ptr %411, align 8
  %__mask_count_0_load1161 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1162 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %412, i32 0, i32 0
  store i64 %__mask_count_0_load1161, ptr %len_ptr1162, align 8
  %__where_result_0_load1163 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1164 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1163, i32 0, i32 1
  %data_header1165 = load ptr, ptr %df_data_ptr1164, align 8
  %413 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1165, i32 0, i32 2
  %414 = load ptr, ptr %413, align 8
  %415 = getelementptr ptr, ptr %414, i64 34
  %416 = load ptr, ptr %415, align 8
  %__mask_count_0_load1166 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1167 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %416, i32 0, i32 0
  store i64 %__mask_count_0_load1166, ptr %len_ptr1167, align 8
  %__where_result_0_load1168 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1169 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1168, i32 0, i32 1
  %data_header1170 = load ptr, ptr %df_data_ptr1169, align 8
  %417 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1170, i32 0, i32 2
  %418 = load ptr, ptr %417, align 8
  %419 = getelementptr ptr, ptr %418, i64 35
  %420 = load ptr, ptr %419, align 8
  %__mask_count_0_load1171 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1172 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %420, i32 0, i32 0
  store i64 %__mask_count_0_load1171, ptr %len_ptr1172, align 8
  %__where_result_0_load1173 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1174 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1173, i32 0, i32 1
  %data_header1175 = load ptr, ptr %df_data_ptr1174, align 8
  %421 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1175, i32 0, i32 2
  %422 = load ptr, ptr %421, align 8
  %423 = getelementptr ptr, ptr %422, i64 36
  %424 = load ptr, ptr %423, align 8
  %__mask_count_0_load1176 = load i64, ptr @__mask_count_0, align 8
  %len_ptr1177 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %424, i32 0, i32 0
  store i64 %__mask_count_0_load1176, ptr %len_ptr1177, align 8
  store i64 0, ptr @__where_res_i_0, align 8
  store i64 0, ptr @__where_i_0, align 8
  br label %for.cond1178

bounds.fail721:                                   ; preds = %for.body701
  %print_err723 = call i32 (ptr, ...) @printf(ptr @err_msg.38)
  ret ptr null

bounds.ok722:                                     ; preds = %for.body701
  %elem_ptr724 = getelementptr i8, ptr %data_ptr713, i64 %resolved_index717
  %loaded_val725 = load i8, ptr %elem_ptr724, align 1
  br i8 %loaded_val725, label %then, label %else

then:                                             ; preds = %bounds.ok722
  %x_load726 = load i64, ptr @__mask_count_0, align 8
  %inc_add727 = add i64 %x_load726, 1
  store i64 %inc_add727, ptr @__mask_count_0, align 8
  br label %ifcont

else:                                             ; preds = %bounds.ok722
  br label %ifcont

ifcont:                                           ; preds = %else, %then
  br label %for.step702

for.cond1178:                                     ; preds = %for.step1180, %for.end703
  %__where_i_0_load1182 = load i64, ptr @__where_i_0, align 8
  %__where_src_0_load1183 = load ptr, ptr @__where_src_0, align 8
  %rowCount_ptr1184 = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %__where_src_0_load1183, i32 0, i32 3
  %rowCount1185 = load i64, ptr %rowCount_ptr1184, align 8
  %icmp_tmp1186 = icmp slt i64 %__where_i_0_load1182, %rowCount1185
  br i1 %icmp_tmp1186, label %for.body1179, label %for.end1181

for.body1179:                                     ; preds = %for.cond1178
  %__where_mask_0_load1187 = load ptr, ptr @__where_mask_0, align 8
  %__where_i_0_load1188 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1189 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_mask_0_load1187, i32 0, i32 0
  %data_field_ptr1190 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %__where_mask_0_load1187, i32 0, i32 2
  %data_ptr1191 = load ptr, ptr %data_field_ptr1190, align 8
  %array_len1192 = load i64, ptr %len_field_ptr1189, align 8
  %index_is_neg1193 = icmp slt i64 %__where_i_0_load1188, 0
  %index_rel1194 = add i64 %__where_i_0_load1188, %array_len1192
  %resolved_index1195 = select i1 %index_is_neg1193, i64 %index_rel1194, i64 %__where_i_0_load1188
  %is_neg1196 = icmp slt i64 %resolved_index1195, 0
  %is_too_big1197 = icmp sge i64 %resolved_index1195, %array_len1192
  %is_invalid1198 = or i1 %is_neg1196, %is_too_big1197
  br i1 %is_invalid1198, label %bounds.fail1199, label %bounds.ok1200

for.step1180:                                     ; preds = %ifcont1206
  %x_load2392 = load i64, ptr @__where_i_0, align 8
  %inc_add2393 = add i64 %x_load2392, 1
  store i64 %inc_add2393, ptr @__where_i_0, align 8
  br label %for.cond1178, !llvm.loop !3

for.end1181:                                      ; preds = %for.cond1178
  %__where_result_0_load2394 = load ptr, ptr @__where_result_0, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr2395 = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %__where_result_0_load2394, ptr %data_ptr2395, align 8
  ret ptr %runtime_obj

bounds.fail1199:                                  ; preds = %for.body1179
  %print_err1201 = call i32 (ptr, ...) @printf(ptr @err_msg.75)
  ret ptr null

bounds.ok1200:                                    ; preds = %for.body1179
  %elem_ptr1202 = getelementptr i8, ptr %data_ptr1191, i64 %resolved_index1195
  %loaded_val1203 = load i8, ptr %elem_ptr1202, align 1
  br i8 %loaded_val1203, label %then1204, label %else1205

then1204:                                         ; preds = %bounds.ok1200
  %__where_result_0_load1207 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1208 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1207, i32 0, i32 1
  %data_header1209 = load ptr, ptr %df_data_ptr1208, align 8
  %425 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1209, i32 0, i32 2
  %426 = load ptr, ptr %425, align 8
  %427 = getelementptr ptr, ptr %426, i64 0
  %428 = load ptr, ptr %427, align 8
  %__where_res_i_0_load = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1210 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %428, i32 0, i32 0
  %data_ptr_ptr1211 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %428, i32 0, i32 2
  %array_len1212 = load i64, ptr %len_ptr1210, align 4
  %data_ptr1213 = load ptr, ptr %data_ptr_ptr1211, align 8
  %429 = icmp slt i64 %__where_res_i_0_load, 0
  %430 = icmp sge i64 %__where_res_i_0_load, %array_len1212
  %is_invalid1214 = or i1 %429, %430
  br i1 %is_invalid1214, label %assign_bounds.fail1215, label %assign_bounds.ok1216

else1205:                                         ; preds = %bounds.ok1200
  br label %ifcont1206

ifcont1206:                                       ; preds = %else1205, %bounds.ok2385
  br label %for.step1180

assign_bounds.fail1215:                           ; preds = %then1204
  %print_err1217 = call i32 (ptr, ...) @printf(ptr @err_msg.76)
  ret ptr null

assign_bounds.ok1216:                             ; preds = %then1204
  %__where_src_0_load1218 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1219 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1218, i32 0, i32 1
  %data_header1220 = load ptr, ptr %df_data_ptr1219, align 8
  %431 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1220, i32 0, i32 2
  %432 = load ptr, ptr %431, align 8
  %433 = getelementptr ptr, ptr %432, i64 0
  %434 = load ptr, ptr %433, align 8
  %__where_i_0_load1221 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1222 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %434, i32 0, i32 0
  %data_field_ptr1223 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %434, i32 0, i32 2
  %data_ptr1224 = load ptr, ptr %data_field_ptr1223, align 8
  %array_len1225 = load i64, ptr %len_field_ptr1222, align 8
  %index_is_neg1226 = icmp slt i64 %__where_i_0_load1221, 0
  %index_rel1227 = add i64 %__where_i_0_load1221, %array_len1225
  %resolved_index1228 = select i1 %index_is_neg1226, i64 %index_rel1227, i64 %__where_i_0_load1221
  %is_neg1229 = icmp slt i64 %resolved_index1228, 0
  %is_too_big1230 = icmp sge i64 %resolved_index1228, %array_len1225
  %is_invalid1231 = or i1 %is_neg1229, %is_too_big1230
  br i1 %is_invalid1231, label %bounds.fail1232, label %bounds.ok1233

bounds.fail1232:                                  ; preds = %assign_bounds.ok1216
  %print_err1234 = call i32 (ptr, ...) @printf(ptr @err_msg.77)
  ret ptr null

bounds.ok1233:                                    ; preds = %assign_bounds.ok1216
  %elem_ptr1235 = getelementptr ptr, ptr %data_ptr1224, i64 %resolved_index1228
  %loaded_val1236 = load ptr, ptr %elem_ptr1235, align 8
  %elem_ptr1237 = getelementptr ptr, ptr %data_ptr1213, i64 %__where_res_i_0_load
  store ptr %loaded_val1236, ptr %elem_ptr1237, align 8
  %__where_result_0_load1238 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1239 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1238, i32 0, i32 1
  %data_header1240 = load ptr, ptr %df_data_ptr1239, align 8
  %435 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1240, i32 0, i32 2
  %436 = load ptr, ptr %435, align 8
  %437 = getelementptr ptr, ptr %436, i64 1
  %438 = load ptr, ptr %437, align 8
  %__where_res_i_0_load1241 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1242 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %438, i32 0, i32 0
  %data_ptr_ptr1243 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %438, i32 0, i32 2
  %array_len1244 = load i64, ptr %len_ptr1242, align 4
  %data_ptr1245 = load ptr, ptr %data_ptr_ptr1243, align 8
  %439 = icmp slt i64 %__where_res_i_0_load1241, 0
  %440 = icmp sge i64 %__where_res_i_0_load1241, %array_len1244
  %is_invalid1246 = or i1 %439, %440
  br i1 %is_invalid1246, label %assign_bounds.fail1247, label %assign_bounds.ok1248

assign_bounds.fail1247:                           ; preds = %bounds.ok1233
  %print_err1249 = call i32 (ptr, ...) @printf(ptr @err_msg.78)
  ret ptr null

assign_bounds.ok1248:                             ; preds = %bounds.ok1233
  %__where_src_0_load1250 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1251 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1250, i32 0, i32 1
  %data_header1252 = load ptr, ptr %df_data_ptr1251, align 8
  %441 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1252, i32 0, i32 2
  %442 = load ptr, ptr %441, align 8
  %443 = getelementptr ptr, ptr %442, i64 1
  %444 = load ptr, ptr %443, align 8
  %__where_i_0_load1253 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1254 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %444, i32 0, i32 0
  %data_field_ptr1255 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %444, i32 0, i32 2
  %data_ptr1256 = load ptr, ptr %data_field_ptr1255, align 8
  %array_len1257 = load i64, ptr %len_field_ptr1254, align 8
  %index_is_neg1258 = icmp slt i64 %__where_i_0_load1253, 0
  %index_rel1259 = add i64 %__where_i_0_load1253, %array_len1257
  %resolved_index1260 = select i1 %index_is_neg1258, i64 %index_rel1259, i64 %__where_i_0_load1253
  %is_neg1261 = icmp slt i64 %resolved_index1260, 0
  %is_too_big1262 = icmp sge i64 %resolved_index1260, %array_len1257
  %is_invalid1263 = or i1 %is_neg1261, %is_too_big1262
  br i1 %is_invalid1263, label %bounds.fail1264, label %bounds.ok1265

bounds.fail1264:                                  ; preds = %assign_bounds.ok1248
  %print_err1266 = call i32 (ptr, ...) @printf(ptr @err_msg.79)
  ret ptr null

bounds.ok1265:                                    ; preds = %assign_bounds.ok1248
  %elem_ptr1267 = getelementptr double, ptr %data_ptr1256, i64 %resolved_index1260
  %loaded_val1268 = load double, ptr %elem_ptr1267, align 8
  %elem_ptr1269 = getelementptr double, ptr %data_ptr1245, i64 %__where_res_i_0_load1241
  store double %loaded_val1268, ptr %elem_ptr1269, align 8
  %__where_result_0_load1270 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1271 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1270, i32 0, i32 1
  %data_header1272 = load ptr, ptr %df_data_ptr1271, align 8
  %445 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1272, i32 0, i32 2
  %446 = load ptr, ptr %445, align 8
  %447 = getelementptr ptr, ptr %446, i64 2
  %448 = load ptr, ptr %447, align 8
  %__where_res_i_0_load1273 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1274 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %448, i32 0, i32 0
  %data_ptr_ptr1275 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %448, i32 0, i32 2
  %array_len1276 = load i64, ptr %len_ptr1274, align 4
  %data_ptr1277 = load ptr, ptr %data_ptr_ptr1275, align 8
  %449 = icmp slt i64 %__where_res_i_0_load1273, 0
  %450 = icmp sge i64 %__where_res_i_0_load1273, %array_len1276
  %is_invalid1278 = or i1 %449, %450
  br i1 %is_invalid1278, label %assign_bounds.fail1279, label %assign_bounds.ok1280

assign_bounds.fail1279:                           ; preds = %bounds.ok1265
  %print_err1281 = call i32 (ptr, ...) @printf(ptr @err_msg.80)
  ret ptr null

assign_bounds.ok1280:                             ; preds = %bounds.ok1265
  %__where_src_0_load1282 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1283 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1282, i32 0, i32 1
  %data_header1284 = load ptr, ptr %df_data_ptr1283, align 8
  %451 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1284, i32 0, i32 2
  %452 = load ptr, ptr %451, align 8
  %453 = getelementptr ptr, ptr %452, i64 2
  %454 = load ptr, ptr %453, align 8
  %__where_i_0_load1285 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1286 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %454, i32 0, i32 0
  %data_field_ptr1287 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %454, i32 0, i32 2
  %data_ptr1288 = load ptr, ptr %data_field_ptr1287, align 8
  %array_len1289 = load i64, ptr %len_field_ptr1286, align 8
  %index_is_neg1290 = icmp slt i64 %__where_i_0_load1285, 0
  %index_rel1291 = add i64 %__where_i_0_load1285, %array_len1289
  %resolved_index1292 = select i1 %index_is_neg1290, i64 %index_rel1291, i64 %__where_i_0_load1285
  %is_neg1293 = icmp slt i64 %resolved_index1292, 0
  %is_too_big1294 = icmp sge i64 %resolved_index1292, %array_len1289
  %is_invalid1295 = or i1 %is_neg1293, %is_too_big1294
  br i1 %is_invalid1295, label %bounds.fail1296, label %bounds.ok1297

bounds.fail1296:                                  ; preds = %assign_bounds.ok1280
  %print_err1298 = call i32 (ptr, ...) @printf(ptr @err_msg.81)
  ret ptr null

bounds.ok1297:                                    ; preds = %assign_bounds.ok1280
  %elem_ptr1299 = getelementptr double, ptr %data_ptr1288, i64 %resolved_index1292
  %loaded_val1300 = load double, ptr %elem_ptr1299, align 8
  %elem_ptr1301 = getelementptr double, ptr %data_ptr1277, i64 %__where_res_i_0_load1273
  store double %loaded_val1300, ptr %elem_ptr1301, align 8
  %__where_result_0_load1302 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1303 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1302, i32 0, i32 1
  %data_header1304 = load ptr, ptr %df_data_ptr1303, align 8
  %455 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1304, i32 0, i32 2
  %456 = load ptr, ptr %455, align 8
  %457 = getelementptr ptr, ptr %456, i64 3
  %458 = load ptr, ptr %457, align 8
  %__where_res_i_0_load1305 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1306 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %458, i32 0, i32 0
  %data_ptr_ptr1307 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %458, i32 0, i32 2
  %array_len1308 = load i64, ptr %len_ptr1306, align 4
  %data_ptr1309 = load ptr, ptr %data_ptr_ptr1307, align 8
  %459 = icmp slt i64 %__where_res_i_0_load1305, 0
  %460 = icmp sge i64 %__where_res_i_0_load1305, %array_len1308
  %is_invalid1310 = or i1 %459, %460
  br i1 %is_invalid1310, label %assign_bounds.fail1311, label %assign_bounds.ok1312

assign_bounds.fail1311:                           ; preds = %bounds.ok1297
  %print_err1313 = call i32 (ptr, ...) @printf(ptr @err_msg.82)
  ret ptr null

assign_bounds.ok1312:                             ; preds = %bounds.ok1297
  %__where_src_0_load1314 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1315 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1314, i32 0, i32 1
  %data_header1316 = load ptr, ptr %df_data_ptr1315, align 8
  %461 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1316, i32 0, i32 2
  %462 = load ptr, ptr %461, align 8
  %463 = getelementptr ptr, ptr %462, i64 3
  %464 = load ptr, ptr %463, align 8
  %__where_i_0_load1317 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1318 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %464, i32 0, i32 0
  %data_field_ptr1319 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %464, i32 0, i32 2
  %data_ptr1320 = load ptr, ptr %data_field_ptr1319, align 8
  %array_len1321 = load i64, ptr %len_field_ptr1318, align 8
  %index_is_neg1322 = icmp slt i64 %__where_i_0_load1317, 0
  %index_rel1323 = add i64 %__where_i_0_load1317, %array_len1321
  %resolved_index1324 = select i1 %index_is_neg1322, i64 %index_rel1323, i64 %__where_i_0_load1317
  %is_neg1325 = icmp slt i64 %resolved_index1324, 0
  %is_too_big1326 = icmp sge i64 %resolved_index1324, %array_len1321
  %is_invalid1327 = or i1 %is_neg1325, %is_too_big1326
  br i1 %is_invalid1327, label %bounds.fail1328, label %bounds.ok1329

bounds.fail1328:                                  ; preds = %assign_bounds.ok1312
  %print_err1330 = call i32 (ptr, ...) @printf(ptr @err_msg.83)
  ret ptr null

bounds.ok1329:                                    ; preds = %assign_bounds.ok1312
  %elem_ptr1331 = getelementptr double, ptr %data_ptr1320, i64 %resolved_index1324
  %loaded_val1332 = load double, ptr %elem_ptr1331, align 8
  %elem_ptr1333 = getelementptr double, ptr %data_ptr1309, i64 %__where_res_i_0_load1305
  store double %loaded_val1332, ptr %elem_ptr1333, align 8
  %__where_result_0_load1334 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1335 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1334, i32 0, i32 1
  %data_header1336 = load ptr, ptr %df_data_ptr1335, align 8
  %465 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1336, i32 0, i32 2
  %466 = load ptr, ptr %465, align 8
  %467 = getelementptr ptr, ptr %466, i64 4
  %468 = load ptr, ptr %467, align 8
  %__where_res_i_0_load1337 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1338 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %468, i32 0, i32 0
  %data_ptr_ptr1339 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %468, i32 0, i32 2
  %array_len1340 = load i64, ptr %len_ptr1338, align 4
  %data_ptr1341 = load ptr, ptr %data_ptr_ptr1339, align 8
  %469 = icmp slt i64 %__where_res_i_0_load1337, 0
  %470 = icmp sge i64 %__where_res_i_0_load1337, %array_len1340
  %is_invalid1342 = or i1 %469, %470
  br i1 %is_invalid1342, label %assign_bounds.fail1343, label %assign_bounds.ok1344

assign_bounds.fail1343:                           ; preds = %bounds.ok1329
  %print_err1345 = call i32 (ptr, ...) @printf(ptr @err_msg.84)
  ret ptr null

assign_bounds.ok1344:                             ; preds = %bounds.ok1329
  %__where_src_0_load1346 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1347 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1346, i32 0, i32 1
  %data_header1348 = load ptr, ptr %df_data_ptr1347, align 8
  %471 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1348, i32 0, i32 2
  %472 = load ptr, ptr %471, align 8
  %473 = getelementptr ptr, ptr %472, i64 4
  %474 = load ptr, ptr %473, align 8
  %__where_i_0_load1349 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1350 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %474, i32 0, i32 0
  %data_field_ptr1351 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %474, i32 0, i32 2
  %data_ptr1352 = load ptr, ptr %data_field_ptr1351, align 8
  %array_len1353 = load i64, ptr %len_field_ptr1350, align 8
  %index_is_neg1354 = icmp slt i64 %__where_i_0_load1349, 0
  %index_rel1355 = add i64 %__where_i_0_load1349, %array_len1353
  %resolved_index1356 = select i1 %index_is_neg1354, i64 %index_rel1355, i64 %__where_i_0_load1349
  %is_neg1357 = icmp slt i64 %resolved_index1356, 0
  %is_too_big1358 = icmp sge i64 %resolved_index1356, %array_len1353
  %is_invalid1359 = or i1 %is_neg1357, %is_too_big1358
  br i1 %is_invalid1359, label %bounds.fail1360, label %bounds.ok1361

bounds.fail1360:                                  ; preds = %assign_bounds.ok1344
  %print_err1362 = call i32 (ptr, ...) @printf(ptr @err_msg.85)
  ret ptr null

bounds.ok1361:                                    ; preds = %assign_bounds.ok1344
  %elem_ptr1363 = getelementptr double, ptr %data_ptr1352, i64 %resolved_index1356
  %loaded_val1364 = load double, ptr %elem_ptr1363, align 8
  %elem_ptr1365 = getelementptr double, ptr %data_ptr1341, i64 %__where_res_i_0_load1337
  store double %loaded_val1364, ptr %elem_ptr1365, align 8
  %__where_result_0_load1366 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1367 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1366, i32 0, i32 1
  %data_header1368 = load ptr, ptr %df_data_ptr1367, align 8
  %475 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1368, i32 0, i32 2
  %476 = load ptr, ptr %475, align 8
  %477 = getelementptr ptr, ptr %476, i64 5
  %478 = load ptr, ptr %477, align 8
  %__where_res_i_0_load1369 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1370 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %478, i32 0, i32 0
  %data_ptr_ptr1371 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %478, i32 0, i32 2
  %array_len1372 = load i64, ptr %len_ptr1370, align 4
  %data_ptr1373 = load ptr, ptr %data_ptr_ptr1371, align 8
  %479 = icmp slt i64 %__where_res_i_0_load1369, 0
  %480 = icmp sge i64 %__where_res_i_0_load1369, %array_len1372
  %is_invalid1374 = or i1 %479, %480
  br i1 %is_invalid1374, label %assign_bounds.fail1375, label %assign_bounds.ok1376

assign_bounds.fail1375:                           ; preds = %bounds.ok1361
  %print_err1377 = call i32 (ptr, ...) @printf(ptr @err_msg.86)
  ret ptr null

assign_bounds.ok1376:                             ; preds = %bounds.ok1361
  %__where_src_0_load1378 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1379 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1378, i32 0, i32 1
  %data_header1380 = load ptr, ptr %df_data_ptr1379, align 8
  %481 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1380, i32 0, i32 2
  %482 = load ptr, ptr %481, align 8
  %483 = getelementptr ptr, ptr %482, i64 5
  %484 = load ptr, ptr %483, align 8
  %__where_i_0_load1381 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1382 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %484, i32 0, i32 0
  %data_field_ptr1383 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %484, i32 0, i32 2
  %data_ptr1384 = load ptr, ptr %data_field_ptr1383, align 8
  %array_len1385 = load i64, ptr %len_field_ptr1382, align 8
  %index_is_neg1386 = icmp slt i64 %__where_i_0_load1381, 0
  %index_rel1387 = add i64 %__where_i_0_load1381, %array_len1385
  %resolved_index1388 = select i1 %index_is_neg1386, i64 %index_rel1387, i64 %__where_i_0_load1381
  %is_neg1389 = icmp slt i64 %resolved_index1388, 0
  %is_too_big1390 = icmp sge i64 %resolved_index1388, %array_len1385
  %is_invalid1391 = or i1 %is_neg1389, %is_too_big1390
  br i1 %is_invalid1391, label %bounds.fail1392, label %bounds.ok1393

bounds.fail1392:                                  ; preds = %assign_bounds.ok1376
  %print_err1394 = call i32 (ptr, ...) @printf(ptr @err_msg.87)
  ret ptr null

bounds.ok1393:                                    ; preds = %assign_bounds.ok1376
  %elem_ptr1395 = getelementptr double, ptr %data_ptr1384, i64 %resolved_index1388
  %loaded_val1396 = load double, ptr %elem_ptr1395, align 8
  %elem_ptr1397 = getelementptr double, ptr %data_ptr1373, i64 %__where_res_i_0_load1369
  store double %loaded_val1396, ptr %elem_ptr1397, align 8
  %__where_result_0_load1398 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1399 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1398, i32 0, i32 1
  %data_header1400 = load ptr, ptr %df_data_ptr1399, align 8
  %485 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1400, i32 0, i32 2
  %486 = load ptr, ptr %485, align 8
  %487 = getelementptr ptr, ptr %486, i64 6
  %488 = load ptr, ptr %487, align 8
  %__where_res_i_0_load1401 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1402 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %488, i32 0, i32 0
  %data_ptr_ptr1403 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %488, i32 0, i32 2
  %array_len1404 = load i64, ptr %len_ptr1402, align 4
  %data_ptr1405 = load ptr, ptr %data_ptr_ptr1403, align 8
  %489 = icmp slt i64 %__where_res_i_0_load1401, 0
  %490 = icmp sge i64 %__where_res_i_0_load1401, %array_len1404
  %is_invalid1406 = or i1 %489, %490
  br i1 %is_invalid1406, label %assign_bounds.fail1407, label %assign_bounds.ok1408

assign_bounds.fail1407:                           ; preds = %bounds.ok1393
  %print_err1409 = call i32 (ptr, ...) @printf(ptr @err_msg.88)
  ret ptr null

assign_bounds.ok1408:                             ; preds = %bounds.ok1393
  %__where_src_0_load1410 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1411 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1410, i32 0, i32 1
  %data_header1412 = load ptr, ptr %df_data_ptr1411, align 8
  %491 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1412, i32 0, i32 2
  %492 = load ptr, ptr %491, align 8
  %493 = getelementptr ptr, ptr %492, i64 6
  %494 = load ptr, ptr %493, align 8
  %__where_i_0_load1413 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1414 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %494, i32 0, i32 0
  %data_field_ptr1415 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %494, i32 0, i32 2
  %data_ptr1416 = load ptr, ptr %data_field_ptr1415, align 8
  %array_len1417 = load i64, ptr %len_field_ptr1414, align 8
  %index_is_neg1418 = icmp slt i64 %__where_i_0_load1413, 0
  %index_rel1419 = add i64 %__where_i_0_load1413, %array_len1417
  %resolved_index1420 = select i1 %index_is_neg1418, i64 %index_rel1419, i64 %__where_i_0_load1413
  %is_neg1421 = icmp slt i64 %resolved_index1420, 0
  %is_too_big1422 = icmp sge i64 %resolved_index1420, %array_len1417
  %is_invalid1423 = or i1 %is_neg1421, %is_too_big1422
  br i1 %is_invalid1423, label %bounds.fail1424, label %bounds.ok1425

bounds.fail1424:                                  ; preds = %assign_bounds.ok1408
  %print_err1426 = call i32 (ptr, ...) @printf(ptr @err_msg.89)
  ret ptr null

bounds.ok1425:                                    ; preds = %assign_bounds.ok1408
  %elem_ptr1427 = getelementptr double, ptr %data_ptr1416, i64 %resolved_index1420
  %loaded_val1428 = load double, ptr %elem_ptr1427, align 8
  %elem_ptr1429 = getelementptr double, ptr %data_ptr1405, i64 %__where_res_i_0_load1401
  store double %loaded_val1428, ptr %elem_ptr1429, align 8
  %__where_result_0_load1430 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1431 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1430, i32 0, i32 1
  %data_header1432 = load ptr, ptr %df_data_ptr1431, align 8
  %495 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1432, i32 0, i32 2
  %496 = load ptr, ptr %495, align 8
  %497 = getelementptr ptr, ptr %496, i64 7
  %498 = load ptr, ptr %497, align 8
  %__where_res_i_0_load1433 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1434 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %498, i32 0, i32 0
  %data_ptr_ptr1435 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %498, i32 0, i32 2
  %array_len1436 = load i64, ptr %len_ptr1434, align 4
  %data_ptr1437 = load ptr, ptr %data_ptr_ptr1435, align 8
  %499 = icmp slt i64 %__where_res_i_0_load1433, 0
  %500 = icmp sge i64 %__where_res_i_0_load1433, %array_len1436
  %is_invalid1438 = or i1 %499, %500
  br i1 %is_invalid1438, label %assign_bounds.fail1439, label %assign_bounds.ok1440

assign_bounds.fail1439:                           ; preds = %bounds.ok1425
  %print_err1441 = call i32 (ptr, ...) @printf(ptr @err_msg.90)
  ret ptr null

assign_bounds.ok1440:                             ; preds = %bounds.ok1425
  %__where_src_0_load1442 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1443 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1442, i32 0, i32 1
  %data_header1444 = load ptr, ptr %df_data_ptr1443, align 8
  %501 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1444, i32 0, i32 2
  %502 = load ptr, ptr %501, align 8
  %503 = getelementptr ptr, ptr %502, i64 7
  %504 = load ptr, ptr %503, align 8
  %__where_i_0_load1445 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1446 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %504, i32 0, i32 0
  %data_field_ptr1447 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %504, i32 0, i32 2
  %data_ptr1448 = load ptr, ptr %data_field_ptr1447, align 8
  %array_len1449 = load i64, ptr %len_field_ptr1446, align 8
  %index_is_neg1450 = icmp slt i64 %__where_i_0_load1445, 0
  %index_rel1451 = add i64 %__where_i_0_load1445, %array_len1449
  %resolved_index1452 = select i1 %index_is_neg1450, i64 %index_rel1451, i64 %__where_i_0_load1445
  %is_neg1453 = icmp slt i64 %resolved_index1452, 0
  %is_too_big1454 = icmp sge i64 %resolved_index1452, %array_len1449
  %is_invalid1455 = or i1 %is_neg1453, %is_too_big1454
  br i1 %is_invalid1455, label %bounds.fail1456, label %bounds.ok1457

bounds.fail1456:                                  ; preds = %assign_bounds.ok1440
  %print_err1458 = call i32 (ptr, ...) @printf(ptr @err_msg.91)
  ret ptr null

bounds.ok1457:                                    ; preds = %assign_bounds.ok1440
  %elem_ptr1459 = getelementptr double, ptr %data_ptr1448, i64 %resolved_index1452
  %loaded_val1460 = load double, ptr %elem_ptr1459, align 8
  %elem_ptr1461 = getelementptr double, ptr %data_ptr1437, i64 %__where_res_i_0_load1433
  store double %loaded_val1460, ptr %elem_ptr1461, align 8
  %__where_result_0_load1462 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1463 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1462, i32 0, i32 1
  %data_header1464 = load ptr, ptr %df_data_ptr1463, align 8
  %505 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1464, i32 0, i32 2
  %506 = load ptr, ptr %505, align 8
  %507 = getelementptr ptr, ptr %506, i64 8
  %508 = load ptr, ptr %507, align 8
  %__where_res_i_0_load1465 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1466 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %508, i32 0, i32 0
  %data_ptr_ptr1467 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %508, i32 0, i32 2
  %array_len1468 = load i64, ptr %len_ptr1466, align 4
  %data_ptr1469 = load ptr, ptr %data_ptr_ptr1467, align 8
  %509 = icmp slt i64 %__where_res_i_0_load1465, 0
  %510 = icmp sge i64 %__where_res_i_0_load1465, %array_len1468
  %is_invalid1470 = or i1 %509, %510
  br i1 %is_invalid1470, label %assign_bounds.fail1471, label %assign_bounds.ok1472

assign_bounds.fail1471:                           ; preds = %bounds.ok1457
  %print_err1473 = call i32 (ptr, ...) @printf(ptr @err_msg.92)
  ret ptr null

assign_bounds.ok1472:                             ; preds = %bounds.ok1457
  %__where_src_0_load1474 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1475 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1474, i32 0, i32 1
  %data_header1476 = load ptr, ptr %df_data_ptr1475, align 8
  %511 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1476, i32 0, i32 2
  %512 = load ptr, ptr %511, align 8
  %513 = getelementptr ptr, ptr %512, i64 8
  %514 = load ptr, ptr %513, align 8
  %__where_i_0_load1477 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1478 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %514, i32 0, i32 0
  %data_field_ptr1479 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %514, i32 0, i32 2
  %data_ptr1480 = load ptr, ptr %data_field_ptr1479, align 8
  %array_len1481 = load i64, ptr %len_field_ptr1478, align 8
  %index_is_neg1482 = icmp slt i64 %__where_i_0_load1477, 0
  %index_rel1483 = add i64 %__where_i_0_load1477, %array_len1481
  %resolved_index1484 = select i1 %index_is_neg1482, i64 %index_rel1483, i64 %__where_i_0_load1477
  %is_neg1485 = icmp slt i64 %resolved_index1484, 0
  %is_too_big1486 = icmp sge i64 %resolved_index1484, %array_len1481
  %is_invalid1487 = or i1 %is_neg1485, %is_too_big1486
  br i1 %is_invalid1487, label %bounds.fail1488, label %bounds.ok1489

bounds.fail1488:                                  ; preds = %assign_bounds.ok1472
  %print_err1490 = call i32 (ptr, ...) @printf(ptr @err_msg.93)
  ret ptr null

bounds.ok1489:                                    ; preds = %assign_bounds.ok1472
  %elem_ptr1491 = getelementptr double, ptr %data_ptr1480, i64 %resolved_index1484
  %loaded_val1492 = load double, ptr %elem_ptr1491, align 8
  %elem_ptr1493 = getelementptr double, ptr %data_ptr1469, i64 %__where_res_i_0_load1465
  store double %loaded_val1492, ptr %elem_ptr1493, align 8
  %__where_result_0_load1494 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1495 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1494, i32 0, i32 1
  %data_header1496 = load ptr, ptr %df_data_ptr1495, align 8
  %515 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1496, i32 0, i32 2
  %516 = load ptr, ptr %515, align 8
  %517 = getelementptr ptr, ptr %516, i64 9
  %518 = load ptr, ptr %517, align 8
  %__where_res_i_0_load1497 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1498 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %518, i32 0, i32 0
  %data_ptr_ptr1499 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %518, i32 0, i32 2
  %array_len1500 = load i64, ptr %len_ptr1498, align 4
  %data_ptr1501 = load ptr, ptr %data_ptr_ptr1499, align 8
  %519 = icmp slt i64 %__where_res_i_0_load1497, 0
  %520 = icmp sge i64 %__where_res_i_0_load1497, %array_len1500
  %is_invalid1502 = or i1 %519, %520
  br i1 %is_invalid1502, label %assign_bounds.fail1503, label %assign_bounds.ok1504

assign_bounds.fail1503:                           ; preds = %bounds.ok1489
  %print_err1505 = call i32 (ptr, ...) @printf(ptr @err_msg.94)
  ret ptr null

assign_bounds.ok1504:                             ; preds = %bounds.ok1489
  %__where_src_0_load1506 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1507 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1506, i32 0, i32 1
  %data_header1508 = load ptr, ptr %df_data_ptr1507, align 8
  %521 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1508, i32 0, i32 2
  %522 = load ptr, ptr %521, align 8
  %523 = getelementptr ptr, ptr %522, i64 9
  %524 = load ptr, ptr %523, align 8
  %__where_i_0_load1509 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1510 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %524, i32 0, i32 0
  %data_field_ptr1511 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %524, i32 0, i32 2
  %data_ptr1512 = load ptr, ptr %data_field_ptr1511, align 8
  %array_len1513 = load i64, ptr %len_field_ptr1510, align 8
  %index_is_neg1514 = icmp slt i64 %__where_i_0_load1509, 0
  %index_rel1515 = add i64 %__where_i_0_load1509, %array_len1513
  %resolved_index1516 = select i1 %index_is_neg1514, i64 %index_rel1515, i64 %__where_i_0_load1509
  %is_neg1517 = icmp slt i64 %resolved_index1516, 0
  %is_too_big1518 = icmp sge i64 %resolved_index1516, %array_len1513
  %is_invalid1519 = or i1 %is_neg1517, %is_too_big1518
  br i1 %is_invalid1519, label %bounds.fail1520, label %bounds.ok1521

bounds.fail1520:                                  ; preds = %assign_bounds.ok1504
  %print_err1522 = call i32 (ptr, ...) @printf(ptr @err_msg.95)
  ret ptr null

bounds.ok1521:                                    ; preds = %assign_bounds.ok1504
  %elem_ptr1523 = getelementptr double, ptr %data_ptr1512, i64 %resolved_index1516
  %loaded_val1524 = load double, ptr %elem_ptr1523, align 8
  %elem_ptr1525 = getelementptr double, ptr %data_ptr1501, i64 %__where_res_i_0_load1497
  store double %loaded_val1524, ptr %elem_ptr1525, align 8
  %__where_result_0_load1526 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1527 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1526, i32 0, i32 1
  %data_header1528 = load ptr, ptr %df_data_ptr1527, align 8
  %525 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1528, i32 0, i32 2
  %526 = load ptr, ptr %525, align 8
  %527 = getelementptr ptr, ptr %526, i64 10
  %528 = load ptr, ptr %527, align 8
  %__where_res_i_0_load1529 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1530 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %528, i32 0, i32 0
  %data_ptr_ptr1531 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %528, i32 0, i32 2
  %array_len1532 = load i64, ptr %len_ptr1530, align 4
  %data_ptr1533 = load ptr, ptr %data_ptr_ptr1531, align 8
  %529 = icmp slt i64 %__where_res_i_0_load1529, 0
  %530 = icmp sge i64 %__where_res_i_0_load1529, %array_len1532
  %is_invalid1534 = or i1 %529, %530
  br i1 %is_invalid1534, label %assign_bounds.fail1535, label %assign_bounds.ok1536

assign_bounds.fail1535:                           ; preds = %bounds.ok1521
  %print_err1537 = call i32 (ptr, ...) @printf(ptr @err_msg.96)
  ret ptr null

assign_bounds.ok1536:                             ; preds = %bounds.ok1521
  %__where_src_0_load1538 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1539 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1538, i32 0, i32 1
  %data_header1540 = load ptr, ptr %df_data_ptr1539, align 8
  %531 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1540, i32 0, i32 2
  %532 = load ptr, ptr %531, align 8
  %533 = getelementptr ptr, ptr %532, i64 10
  %534 = load ptr, ptr %533, align 8
  %__where_i_0_load1541 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1542 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %534, i32 0, i32 0
  %data_field_ptr1543 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %534, i32 0, i32 2
  %data_ptr1544 = load ptr, ptr %data_field_ptr1543, align 8
  %array_len1545 = load i64, ptr %len_field_ptr1542, align 8
  %index_is_neg1546 = icmp slt i64 %__where_i_0_load1541, 0
  %index_rel1547 = add i64 %__where_i_0_load1541, %array_len1545
  %resolved_index1548 = select i1 %index_is_neg1546, i64 %index_rel1547, i64 %__where_i_0_load1541
  %is_neg1549 = icmp slt i64 %resolved_index1548, 0
  %is_too_big1550 = icmp sge i64 %resolved_index1548, %array_len1545
  %is_invalid1551 = or i1 %is_neg1549, %is_too_big1550
  br i1 %is_invalid1551, label %bounds.fail1552, label %bounds.ok1553

bounds.fail1552:                                  ; preds = %assign_bounds.ok1536
  %print_err1554 = call i32 (ptr, ...) @printf(ptr @err_msg.97)
  ret ptr null

bounds.ok1553:                                    ; preds = %assign_bounds.ok1536
  %elem_ptr1555 = getelementptr double, ptr %data_ptr1544, i64 %resolved_index1548
  %loaded_val1556 = load double, ptr %elem_ptr1555, align 8
  %elem_ptr1557 = getelementptr double, ptr %data_ptr1533, i64 %__where_res_i_0_load1529
  store double %loaded_val1556, ptr %elem_ptr1557, align 8
  %__where_result_0_load1558 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1559 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1558, i32 0, i32 1
  %data_header1560 = load ptr, ptr %df_data_ptr1559, align 8
  %535 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1560, i32 0, i32 2
  %536 = load ptr, ptr %535, align 8
  %537 = getelementptr ptr, ptr %536, i64 11
  %538 = load ptr, ptr %537, align 8
  %__where_res_i_0_load1561 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1562 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %538, i32 0, i32 0
  %data_ptr_ptr1563 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %538, i32 0, i32 2
  %array_len1564 = load i64, ptr %len_ptr1562, align 4
  %data_ptr1565 = load ptr, ptr %data_ptr_ptr1563, align 8
  %539 = icmp slt i64 %__where_res_i_0_load1561, 0
  %540 = icmp sge i64 %__where_res_i_0_load1561, %array_len1564
  %is_invalid1566 = or i1 %539, %540
  br i1 %is_invalid1566, label %assign_bounds.fail1567, label %assign_bounds.ok1568

assign_bounds.fail1567:                           ; preds = %bounds.ok1553
  %print_err1569 = call i32 (ptr, ...) @printf(ptr @err_msg.98)
  ret ptr null

assign_bounds.ok1568:                             ; preds = %bounds.ok1553
  %__where_src_0_load1570 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1571 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1570, i32 0, i32 1
  %data_header1572 = load ptr, ptr %df_data_ptr1571, align 8
  %541 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1572, i32 0, i32 2
  %542 = load ptr, ptr %541, align 8
  %543 = getelementptr ptr, ptr %542, i64 11
  %544 = load ptr, ptr %543, align 8
  %__where_i_0_load1573 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1574 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %544, i32 0, i32 0
  %data_field_ptr1575 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %544, i32 0, i32 2
  %data_ptr1576 = load ptr, ptr %data_field_ptr1575, align 8
  %array_len1577 = load i64, ptr %len_field_ptr1574, align 8
  %index_is_neg1578 = icmp slt i64 %__where_i_0_load1573, 0
  %index_rel1579 = add i64 %__where_i_0_load1573, %array_len1577
  %resolved_index1580 = select i1 %index_is_neg1578, i64 %index_rel1579, i64 %__where_i_0_load1573
  %is_neg1581 = icmp slt i64 %resolved_index1580, 0
  %is_too_big1582 = icmp sge i64 %resolved_index1580, %array_len1577
  %is_invalid1583 = or i1 %is_neg1581, %is_too_big1582
  br i1 %is_invalid1583, label %bounds.fail1584, label %bounds.ok1585

bounds.fail1584:                                  ; preds = %assign_bounds.ok1568
  %print_err1586 = call i32 (ptr, ...) @printf(ptr @err_msg.99)
  ret ptr null

bounds.ok1585:                                    ; preds = %assign_bounds.ok1568
  %elem_ptr1587 = getelementptr double, ptr %data_ptr1576, i64 %resolved_index1580
  %loaded_val1588 = load double, ptr %elem_ptr1587, align 8
  %elem_ptr1589 = getelementptr double, ptr %data_ptr1565, i64 %__where_res_i_0_load1561
  store double %loaded_val1588, ptr %elem_ptr1589, align 8
  %__where_result_0_load1590 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1591 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1590, i32 0, i32 1
  %data_header1592 = load ptr, ptr %df_data_ptr1591, align 8
  %545 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1592, i32 0, i32 2
  %546 = load ptr, ptr %545, align 8
  %547 = getelementptr ptr, ptr %546, i64 12
  %548 = load ptr, ptr %547, align 8
  %__where_res_i_0_load1593 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1594 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %548, i32 0, i32 0
  %data_ptr_ptr1595 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %548, i32 0, i32 2
  %array_len1596 = load i64, ptr %len_ptr1594, align 4
  %data_ptr1597 = load ptr, ptr %data_ptr_ptr1595, align 8
  %549 = icmp slt i64 %__where_res_i_0_load1593, 0
  %550 = icmp sge i64 %__where_res_i_0_load1593, %array_len1596
  %is_invalid1598 = or i1 %549, %550
  br i1 %is_invalid1598, label %assign_bounds.fail1599, label %assign_bounds.ok1600

assign_bounds.fail1599:                           ; preds = %bounds.ok1585
  %print_err1601 = call i32 (ptr, ...) @printf(ptr @err_msg.100)
  ret ptr null

assign_bounds.ok1600:                             ; preds = %bounds.ok1585
  %__where_src_0_load1602 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1603 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1602, i32 0, i32 1
  %data_header1604 = load ptr, ptr %df_data_ptr1603, align 8
  %551 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1604, i32 0, i32 2
  %552 = load ptr, ptr %551, align 8
  %553 = getelementptr ptr, ptr %552, i64 12
  %554 = load ptr, ptr %553, align 8
  %__where_i_0_load1605 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1606 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %554, i32 0, i32 0
  %data_field_ptr1607 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %554, i32 0, i32 2
  %data_ptr1608 = load ptr, ptr %data_field_ptr1607, align 8
  %array_len1609 = load i64, ptr %len_field_ptr1606, align 8
  %index_is_neg1610 = icmp slt i64 %__where_i_0_load1605, 0
  %index_rel1611 = add i64 %__where_i_0_load1605, %array_len1609
  %resolved_index1612 = select i1 %index_is_neg1610, i64 %index_rel1611, i64 %__where_i_0_load1605
  %is_neg1613 = icmp slt i64 %resolved_index1612, 0
  %is_too_big1614 = icmp sge i64 %resolved_index1612, %array_len1609
  %is_invalid1615 = or i1 %is_neg1613, %is_too_big1614
  br i1 %is_invalid1615, label %bounds.fail1616, label %bounds.ok1617

bounds.fail1616:                                  ; preds = %assign_bounds.ok1600
  %print_err1618 = call i32 (ptr, ...) @printf(ptr @err_msg.101)
  ret ptr null

bounds.ok1617:                                    ; preds = %assign_bounds.ok1600
  %elem_ptr1619 = getelementptr double, ptr %data_ptr1608, i64 %resolved_index1612
  %loaded_val1620 = load double, ptr %elem_ptr1619, align 8
  %elem_ptr1621 = getelementptr double, ptr %data_ptr1597, i64 %__where_res_i_0_load1593
  store double %loaded_val1620, ptr %elem_ptr1621, align 8
  %__where_result_0_load1622 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1623 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1622, i32 0, i32 1
  %data_header1624 = load ptr, ptr %df_data_ptr1623, align 8
  %555 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1624, i32 0, i32 2
  %556 = load ptr, ptr %555, align 8
  %557 = getelementptr ptr, ptr %556, i64 13
  %558 = load ptr, ptr %557, align 8
  %__where_res_i_0_load1625 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1626 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %558, i32 0, i32 0
  %data_ptr_ptr1627 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %558, i32 0, i32 2
  %array_len1628 = load i64, ptr %len_ptr1626, align 4
  %data_ptr1629 = load ptr, ptr %data_ptr_ptr1627, align 8
  %559 = icmp slt i64 %__where_res_i_0_load1625, 0
  %560 = icmp sge i64 %__where_res_i_0_load1625, %array_len1628
  %is_invalid1630 = or i1 %559, %560
  br i1 %is_invalid1630, label %assign_bounds.fail1631, label %assign_bounds.ok1632

assign_bounds.fail1631:                           ; preds = %bounds.ok1617
  %print_err1633 = call i32 (ptr, ...) @printf(ptr @err_msg.102)
  ret ptr null

assign_bounds.ok1632:                             ; preds = %bounds.ok1617
  %__where_src_0_load1634 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1635 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1634, i32 0, i32 1
  %data_header1636 = load ptr, ptr %df_data_ptr1635, align 8
  %561 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1636, i32 0, i32 2
  %562 = load ptr, ptr %561, align 8
  %563 = getelementptr ptr, ptr %562, i64 13
  %564 = load ptr, ptr %563, align 8
  %__where_i_0_load1637 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1638 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %564, i32 0, i32 0
  %data_field_ptr1639 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %564, i32 0, i32 2
  %data_ptr1640 = load ptr, ptr %data_field_ptr1639, align 8
  %array_len1641 = load i64, ptr %len_field_ptr1638, align 8
  %index_is_neg1642 = icmp slt i64 %__where_i_0_load1637, 0
  %index_rel1643 = add i64 %__where_i_0_load1637, %array_len1641
  %resolved_index1644 = select i1 %index_is_neg1642, i64 %index_rel1643, i64 %__where_i_0_load1637
  %is_neg1645 = icmp slt i64 %resolved_index1644, 0
  %is_too_big1646 = icmp sge i64 %resolved_index1644, %array_len1641
  %is_invalid1647 = or i1 %is_neg1645, %is_too_big1646
  br i1 %is_invalid1647, label %bounds.fail1648, label %bounds.ok1649

bounds.fail1648:                                  ; preds = %assign_bounds.ok1632
  %print_err1650 = call i32 (ptr, ...) @printf(ptr @err_msg.103)
  ret ptr null

bounds.ok1649:                                    ; preds = %assign_bounds.ok1632
  %elem_ptr1651 = getelementptr double, ptr %data_ptr1640, i64 %resolved_index1644
  %loaded_val1652 = load double, ptr %elem_ptr1651, align 8
  %elem_ptr1653 = getelementptr double, ptr %data_ptr1629, i64 %__where_res_i_0_load1625
  store double %loaded_val1652, ptr %elem_ptr1653, align 8
  %__where_result_0_load1654 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1655 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1654, i32 0, i32 1
  %data_header1656 = load ptr, ptr %df_data_ptr1655, align 8
  %565 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1656, i32 0, i32 2
  %566 = load ptr, ptr %565, align 8
  %567 = getelementptr ptr, ptr %566, i64 14
  %568 = load ptr, ptr %567, align 8
  %__where_res_i_0_load1657 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1658 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %568, i32 0, i32 0
  %data_ptr_ptr1659 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %568, i32 0, i32 2
  %array_len1660 = load i64, ptr %len_ptr1658, align 4
  %data_ptr1661 = load ptr, ptr %data_ptr_ptr1659, align 8
  %569 = icmp slt i64 %__where_res_i_0_load1657, 0
  %570 = icmp sge i64 %__where_res_i_0_load1657, %array_len1660
  %is_invalid1662 = or i1 %569, %570
  br i1 %is_invalid1662, label %assign_bounds.fail1663, label %assign_bounds.ok1664

assign_bounds.fail1663:                           ; preds = %bounds.ok1649
  %print_err1665 = call i32 (ptr, ...) @printf(ptr @err_msg.104)
  ret ptr null

assign_bounds.ok1664:                             ; preds = %bounds.ok1649
  %__where_src_0_load1666 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1667 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1666, i32 0, i32 1
  %data_header1668 = load ptr, ptr %df_data_ptr1667, align 8
  %571 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1668, i32 0, i32 2
  %572 = load ptr, ptr %571, align 8
  %573 = getelementptr ptr, ptr %572, i64 14
  %574 = load ptr, ptr %573, align 8
  %__where_i_0_load1669 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1670 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %574, i32 0, i32 0
  %data_field_ptr1671 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %574, i32 0, i32 2
  %data_ptr1672 = load ptr, ptr %data_field_ptr1671, align 8
  %array_len1673 = load i64, ptr %len_field_ptr1670, align 8
  %index_is_neg1674 = icmp slt i64 %__where_i_0_load1669, 0
  %index_rel1675 = add i64 %__where_i_0_load1669, %array_len1673
  %resolved_index1676 = select i1 %index_is_neg1674, i64 %index_rel1675, i64 %__where_i_0_load1669
  %is_neg1677 = icmp slt i64 %resolved_index1676, 0
  %is_too_big1678 = icmp sge i64 %resolved_index1676, %array_len1673
  %is_invalid1679 = or i1 %is_neg1677, %is_too_big1678
  br i1 %is_invalid1679, label %bounds.fail1680, label %bounds.ok1681

bounds.fail1680:                                  ; preds = %assign_bounds.ok1664
  %print_err1682 = call i32 (ptr, ...) @printf(ptr @err_msg.105)
  ret ptr null

bounds.ok1681:                                    ; preds = %assign_bounds.ok1664
  %elem_ptr1683 = getelementptr double, ptr %data_ptr1672, i64 %resolved_index1676
  %loaded_val1684 = load double, ptr %elem_ptr1683, align 8
  %elem_ptr1685 = getelementptr double, ptr %data_ptr1661, i64 %__where_res_i_0_load1657
  store double %loaded_val1684, ptr %elem_ptr1685, align 8
  %__where_result_0_load1686 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1687 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1686, i32 0, i32 1
  %data_header1688 = load ptr, ptr %df_data_ptr1687, align 8
  %575 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1688, i32 0, i32 2
  %576 = load ptr, ptr %575, align 8
  %577 = getelementptr ptr, ptr %576, i64 15
  %578 = load ptr, ptr %577, align 8
  %__where_res_i_0_load1689 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1690 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %578, i32 0, i32 0
  %data_ptr_ptr1691 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %578, i32 0, i32 2
  %array_len1692 = load i64, ptr %len_ptr1690, align 4
  %data_ptr1693 = load ptr, ptr %data_ptr_ptr1691, align 8
  %579 = icmp slt i64 %__where_res_i_0_load1689, 0
  %580 = icmp sge i64 %__where_res_i_0_load1689, %array_len1692
  %is_invalid1694 = or i1 %579, %580
  br i1 %is_invalid1694, label %assign_bounds.fail1695, label %assign_bounds.ok1696

assign_bounds.fail1695:                           ; preds = %bounds.ok1681
  %print_err1697 = call i32 (ptr, ...) @printf(ptr @err_msg.106)
  ret ptr null

assign_bounds.ok1696:                             ; preds = %bounds.ok1681
  %__where_src_0_load1698 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1699 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1698, i32 0, i32 1
  %data_header1700 = load ptr, ptr %df_data_ptr1699, align 8
  %581 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1700, i32 0, i32 2
  %582 = load ptr, ptr %581, align 8
  %583 = getelementptr ptr, ptr %582, i64 15
  %584 = load ptr, ptr %583, align 8
  %__where_i_0_load1701 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1702 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %584, i32 0, i32 0
  %data_field_ptr1703 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %584, i32 0, i32 2
  %data_ptr1704 = load ptr, ptr %data_field_ptr1703, align 8
  %array_len1705 = load i64, ptr %len_field_ptr1702, align 8
  %index_is_neg1706 = icmp slt i64 %__where_i_0_load1701, 0
  %index_rel1707 = add i64 %__where_i_0_load1701, %array_len1705
  %resolved_index1708 = select i1 %index_is_neg1706, i64 %index_rel1707, i64 %__where_i_0_load1701
  %is_neg1709 = icmp slt i64 %resolved_index1708, 0
  %is_too_big1710 = icmp sge i64 %resolved_index1708, %array_len1705
  %is_invalid1711 = or i1 %is_neg1709, %is_too_big1710
  br i1 %is_invalid1711, label %bounds.fail1712, label %bounds.ok1713

bounds.fail1712:                                  ; preds = %assign_bounds.ok1696
  %print_err1714 = call i32 (ptr, ...) @printf(ptr @err_msg.107)
  ret ptr null

bounds.ok1713:                                    ; preds = %assign_bounds.ok1696
  %elem_ptr1715 = getelementptr double, ptr %data_ptr1704, i64 %resolved_index1708
  %loaded_val1716 = load double, ptr %elem_ptr1715, align 8
  %elem_ptr1717 = getelementptr double, ptr %data_ptr1693, i64 %__where_res_i_0_load1689
  store double %loaded_val1716, ptr %elem_ptr1717, align 8
  %__where_result_0_load1718 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1719 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1718, i32 0, i32 1
  %data_header1720 = load ptr, ptr %df_data_ptr1719, align 8
  %585 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1720, i32 0, i32 2
  %586 = load ptr, ptr %585, align 8
  %587 = getelementptr ptr, ptr %586, i64 16
  %588 = load ptr, ptr %587, align 8
  %__where_res_i_0_load1721 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1722 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %588, i32 0, i32 0
  %data_ptr_ptr1723 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %588, i32 0, i32 2
  %array_len1724 = load i64, ptr %len_ptr1722, align 4
  %data_ptr1725 = load ptr, ptr %data_ptr_ptr1723, align 8
  %589 = icmp slt i64 %__where_res_i_0_load1721, 0
  %590 = icmp sge i64 %__where_res_i_0_load1721, %array_len1724
  %is_invalid1726 = or i1 %589, %590
  br i1 %is_invalid1726, label %assign_bounds.fail1727, label %assign_bounds.ok1728

assign_bounds.fail1727:                           ; preds = %bounds.ok1713
  %print_err1729 = call i32 (ptr, ...) @printf(ptr @err_msg.108)
  ret ptr null

assign_bounds.ok1728:                             ; preds = %bounds.ok1713
  %__where_src_0_load1730 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1731 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1730, i32 0, i32 1
  %data_header1732 = load ptr, ptr %df_data_ptr1731, align 8
  %591 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1732, i32 0, i32 2
  %592 = load ptr, ptr %591, align 8
  %593 = getelementptr ptr, ptr %592, i64 16
  %594 = load ptr, ptr %593, align 8
  %__where_i_0_load1733 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1734 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %594, i32 0, i32 0
  %data_field_ptr1735 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %594, i32 0, i32 2
  %data_ptr1736 = load ptr, ptr %data_field_ptr1735, align 8
  %array_len1737 = load i64, ptr %len_field_ptr1734, align 8
  %index_is_neg1738 = icmp slt i64 %__where_i_0_load1733, 0
  %index_rel1739 = add i64 %__where_i_0_load1733, %array_len1737
  %resolved_index1740 = select i1 %index_is_neg1738, i64 %index_rel1739, i64 %__where_i_0_load1733
  %is_neg1741 = icmp slt i64 %resolved_index1740, 0
  %is_too_big1742 = icmp sge i64 %resolved_index1740, %array_len1737
  %is_invalid1743 = or i1 %is_neg1741, %is_too_big1742
  br i1 %is_invalid1743, label %bounds.fail1744, label %bounds.ok1745

bounds.fail1744:                                  ; preds = %assign_bounds.ok1728
  %print_err1746 = call i32 (ptr, ...) @printf(ptr @err_msg.109)
  ret ptr null

bounds.ok1745:                                    ; preds = %assign_bounds.ok1728
  %elem_ptr1747 = getelementptr double, ptr %data_ptr1736, i64 %resolved_index1740
  %loaded_val1748 = load double, ptr %elem_ptr1747, align 8
  %elem_ptr1749 = getelementptr double, ptr %data_ptr1725, i64 %__where_res_i_0_load1721
  store double %loaded_val1748, ptr %elem_ptr1749, align 8
  %__where_result_0_load1750 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1751 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1750, i32 0, i32 1
  %data_header1752 = load ptr, ptr %df_data_ptr1751, align 8
  %595 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1752, i32 0, i32 2
  %596 = load ptr, ptr %595, align 8
  %597 = getelementptr ptr, ptr %596, i64 17
  %598 = load ptr, ptr %597, align 8
  %__where_res_i_0_load1753 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1754 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %598, i32 0, i32 0
  %data_ptr_ptr1755 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %598, i32 0, i32 2
  %array_len1756 = load i64, ptr %len_ptr1754, align 4
  %data_ptr1757 = load ptr, ptr %data_ptr_ptr1755, align 8
  %599 = icmp slt i64 %__where_res_i_0_load1753, 0
  %600 = icmp sge i64 %__where_res_i_0_load1753, %array_len1756
  %is_invalid1758 = or i1 %599, %600
  br i1 %is_invalid1758, label %assign_bounds.fail1759, label %assign_bounds.ok1760

assign_bounds.fail1759:                           ; preds = %bounds.ok1745
  %print_err1761 = call i32 (ptr, ...) @printf(ptr @err_msg.110)
  ret ptr null

assign_bounds.ok1760:                             ; preds = %bounds.ok1745
  %__where_src_0_load1762 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1763 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1762, i32 0, i32 1
  %data_header1764 = load ptr, ptr %df_data_ptr1763, align 8
  %601 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1764, i32 0, i32 2
  %602 = load ptr, ptr %601, align 8
  %603 = getelementptr ptr, ptr %602, i64 17
  %604 = load ptr, ptr %603, align 8
  %__where_i_0_load1765 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1766 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %604, i32 0, i32 0
  %data_field_ptr1767 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %604, i32 0, i32 2
  %data_ptr1768 = load ptr, ptr %data_field_ptr1767, align 8
  %array_len1769 = load i64, ptr %len_field_ptr1766, align 8
  %index_is_neg1770 = icmp slt i64 %__where_i_0_load1765, 0
  %index_rel1771 = add i64 %__where_i_0_load1765, %array_len1769
  %resolved_index1772 = select i1 %index_is_neg1770, i64 %index_rel1771, i64 %__where_i_0_load1765
  %is_neg1773 = icmp slt i64 %resolved_index1772, 0
  %is_too_big1774 = icmp sge i64 %resolved_index1772, %array_len1769
  %is_invalid1775 = or i1 %is_neg1773, %is_too_big1774
  br i1 %is_invalid1775, label %bounds.fail1776, label %bounds.ok1777

bounds.fail1776:                                  ; preds = %assign_bounds.ok1760
  %print_err1778 = call i32 (ptr, ...) @printf(ptr @err_msg.111)
  ret ptr null

bounds.ok1777:                                    ; preds = %assign_bounds.ok1760
  %elem_ptr1779 = getelementptr double, ptr %data_ptr1768, i64 %resolved_index1772
  %loaded_val1780 = load double, ptr %elem_ptr1779, align 8
  %elem_ptr1781 = getelementptr double, ptr %data_ptr1757, i64 %__where_res_i_0_load1753
  store double %loaded_val1780, ptr %elem_ptr1781, align 8
  %__where_result_0_load1782 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1783 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1782, i32 0, i32 1
  %data_header1784 = load ptr, ptr %df_data_ptr1783, align 8
  %605 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1784, i32 0, i32 2
  %606 = load ptr, ptr %605, align 8
  %607 = getelementptr ptr, ptr %606, i64 18
  %608 = load ptr, ptr %607, align 8
  %__where_res_i_0_load1785 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1786 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %608, i32 0, i32 0
  %data_ptr_ptr1787 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %608, i32 0, i32 2
  %array_len1788 = load i64, ptr %len_ptr1786, align 4
  %data_ptr1789 = load ptr, ptr %data_ptr_ptr1787, align 8
  %609 = icmp slt i64 %__where_res_i_0_load1785, 0
  %610 = icmp sge i64 %__where_res_i_0_load1785, %array_len1788
  %is_invalid1790 = or i1 %609, %610
  br i1 %is_invalid1790, label %assign_bounds.fail1791, label %assign_bounds.ok1792

assign_bounds.fail1791:                           ; preds = %bounds.ok1777
  %print_err1793 = call i32 (ptr, ...) @printf(ptr @err_msg.112)
  ret ptr null

assign_bounds.ok1792:                             ; preds = %bounds.ok1777
  %__where_src_0_load1794 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1795 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1794, i32 0, i32 1
  %data_header1796 = load ptr, ptr %df_data_ptr1795, align 8
  %611 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1796, i32 0, i32 2
  %612 = load ptr, ptr %611, align 8
  %613 = getelementptr ptr, ptr %612, i64 18
  %614 = load ptr, ptr %613, align 8
  %__where_i_0_load1797 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1798 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %614, i32 0, i32 0
  %data_field_ptr1799 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %614, i32 0, i32 2
  %data_ptr1800 = load ptr, ptr %data_field_ptr1799, align 8
  %array_len1801 = load i64, ptr %len_field_ptr1798, align 8
  %index_is_neg1802 = icmp slt i64 %__where_i_0_load1797, 0
  %index_rel1803 = add i64 %__where_i_0_load1797, %array_len1801
  %resolved_index1804 = select i1 %index_is_neg1802, i64 %index_rel1803, i64 %__where_i_0_load1797
  %is_neg1805 = icmp slt i64 %resolved_index1804, 0
  %is_too_big1806 = icmp sge i64 %resolved_index1804, %array_len1801
  %is_invalid1807 = or i1 %is_neg1805, %is_too_big1806
  br i1 %is_invalid1807, label %bounds.fail1808, label %bounds.ok1809

bounds.fail1808:                                  ; preds = %assign_bounds.ok1792
  %print_err1810 = call i32 (ptr, ...) @printf(ptr @err_msg.113)
  ret ptr null

bounds.ok1809:                                    ; preds = %assign_bounds.ok1792
  %elem_ptr1811 = getelementptr double, ptr %data_ptr1800, i64 %resolved_index1804
  %loaded_val1812 = load double, ptr %elem_ptr1811, align 8
  %elem_ptr1813 = getelementptr double, ptr %data_ptr1789, i64 %__where_res_i_0_load1785
  store double %loaded_val1812, ptr %elem_ptr1813, align 8
  %__where_result_0_load1814 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1815 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1814, i32 0, i32 1
  %data_header1816 = load ptr, ptr %df_data_ptr1815, align 8
  %615 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1816, i32 0, i32 2
  %616 = load ptr, ptr %615, align 8
  %617 = getelementptr ptr, ptr %616, i64 19
  %618 = load ptr, ptr %617, align 8
  %__where_res_i_0_load1817 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1818 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %618, i32 0, i32 0
  %data_ptr_ptr1819 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %618, i32 0, i32 2
  %array_len1820 = load i64, ptr %len_ptr1818, align 4
  %data_ptr1821 = load ptr, ptr %data_ptr_ptr1819, align 8
  %619 = icmp slt i64 %__where_res_i_0_load1817, 0
  %620 = icmp sge i64 %__where_res_i_0_load1817, %array_len1820
  %is_invalid1822 = or i1 %619, %620
  br i1 %is_invalid1822, label %assign_bounds.fail1823, label %assign_bounds.ok1824

assign_bounds.fail1823:                           ; preds = %bounds.ok1809
  %print_err1825 = call i32 (ptr, ...) @printf(ptr @err_msg.114)
  ret ptr null

assign_bounds.ok1824:                             ; preds = %bounds.ok1809
  %__where_src_0_load1826 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1827 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1826, i32 0, i32 1
  %data_header1828 = load ptr, ptr %df_data_ptr1827, align 8
  %621 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1828, i32 0, i32 2
  %622 = load ptr, ptr %621, align 8
  %623 = getelementptr ptr, ptr %622, i64 19
  %624 = load ptr, ptr %623, align 8
  %__where_i_0_load1829 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1830 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %624, i32 0, i32 0
  %data_field_ptr1831 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %624, i32 0, i32 2
  %data_ptr1832 = load ptr, ptr %data_field_ptr1831, align 8
  %array_len1833 = load i64, ptr %len_field_ptr1830, align 8
  %index_is_neg1834 = icmp slt i64 %__where_i_0_load1829, 0
  %index_rel1835 = add i64 %__where_i_0_load1829, %array_len1833
  %resolved_index1836 = select i1 %index_is_neg1834, i64 %index_rel1835, i64 %__where_i_0_load1829
  %is_neg1837 = icmp slt i64 %resolved_index1836, 0
  %is_too_big1838 = icmp sge i64 %resolved_index1836, %array_len1833
  %is_invalid1839 = or i1 %is_neg1837, %is_too_big1838
  br i1 %is_invalid1839, label %bounds.fail1840, label %bounds.ok1841

bounds.fail1840:                                  ; preds = %assign_bounds.ok1824
  %print_err1842 = call i32 (ptr, ...) @printf(ptr @err_msg.115)
  ret ptr null

bounds.ok1841:                                    ; preds = %assign_bounds.ok1824
  %elem_ptr1843 = getelementptr double, ptr %data_ptr1832, i64 %resolved_index1836
  %loaded_val1844 = load double, ptr %elem_ptr1843, align 8
  %elem_ptr1845 = getelementptr double, ptr %data_ptr1821, i64 %__where_res_i_0_load1817
  store double %loaded_val1844, ptr %elem_ptr1845, align 8
  %__where_result_0_load1846 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1847 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1846, i32 0, i32 1
  %data_header1848 = load ptr, ptr %df_data_ptr1847, align 8
  %625 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1848, i32 0, i32 2
  %626 = load ptr, ptr %625, align 8
  %627 = getelementptr ptr, ptr %626, i64 20
  %628 = load ptr, ptr %627, align 8
  %__where_res_i_0_load1849 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1850 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %628, i32 0, i32 0
  %data_ptr_ptr1851 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %628, i32 0, i32 2
  %array_len1852 = load i64, ptr %len_ptr1850, align 4
  %data_ptr1853 = load ptr, ptr %data_ptr_ptr1851, align 8
  %629 = icmp slt i64 %__where_res_i_0_load1849, 0
  %630 = icmp sge i64 %__where_res_i_0_load1849, %array_len1852
  %is_invalid1854 = or i1 %629, %630
  br i1 %is_invalid1854, label %assign_bounds.fail1855, label %assign_bounds.ok1856

assign_bounds.fail1855:                           ; preds = %bounds.ok1841
  %print_err1857 = call i32 (ptr, ...) @printf(ptr @err_msg.116)
  ret ptr null

assign_bounds.ok1856:                             ; preds = %bounds.ok1841
  %__where_src_0_load1858 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1859 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1858, i32 0, i32 1
  %data_header1860 = load ptr, ptr %df_data_ptr1859, align 8
  %631 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1860, i32 0, i32 2
  %632 = load ptr, ptr %631, align 8
  %633 = getelementptr ptr, ptr %632, i64 20
  %634 = load ptr, ptr %633, align 8
  %__where_i_0_load1861 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1862 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %634, i32 0, i32 0
  %data_field_ptr1863 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %634, i32 0, i32 2
  %data_ptr1864 = load ptr, ptr %data_field_ptr1863, align 8
  %array_len1865 = load i64, ptr %len_field_ptr1862, align 8
  %index_is_neg1866 = icmp slt i64 %__where_i_0_load1861, 0
  %index_rel1867 = add i64 %__where_i_0_load1861, %array_len1865
  %resolved_index1868 = select i1 %index_is_neg1866, i64 %index_rel1867, i64 %__where_i_0_load1861
  %is_neg1869 = icmp slt i64 %resolved_index1868, 0
  %is_too_big1870 = icmp sge i64 %resolved_index1868, %array_len1865
  %is_invalid1871 = or i1 %is_neg1869, %is_too_big1870
  br i1 %is_invalid1871, label %bounds.fail1872, label %bounds.ok1873

bounds.fail1872:                                  ; preds = %assign_bounds.ok1856
  %print_err1874 = call i32 (ptr, ...) @printf(ptr @err_msg.117)
  ret ptr null

bounds.ok1873:                                    ; preds = %assign_bounds.ok1856
  %elem_ptr1875 = getelementptr i64, ptr %data_ptr1864, i64 %resolved_index1868
  %loaded_val1876 = load i64, ptr %elem_ptr1875, align 8
  %elem_ptr1877 = getelementptr i64, ptr %data_ptr1853, i64 %__where_res_i_0_load1849
  store i64 %loaded_val1876, ptr %elem_ptr1877, align 4
  %__where_result_0_load1878 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1879 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1878, i32 0, i32 1
  %data_header1880 = load ptr, ptr %df_data_ptr1879, align 8
  %635 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1880, i32 0, i32 2
  %636 = load ptr, ptr %635, align 8
  %637 = getelementptr ptr, ptr %636, i64 21
  %638 = load ptr, ptr %637, align 8
  %__where_res_i_0_load1881 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1882 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %638, i32 0, i32 0
  %data_ptr_ptr1883 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %638, i32 0, i32 2
  %array_len1884 = load i64, ptr %len_ptr1882, align 4
  %data_ptr1885 = load ptr, ptr %data_ptr_ptr1883, align 8
  %639 = icmp slt i64 %__where_res_i_0_load1881, 0
  %640 = icmp sge i64 %__where_res_i_0_load1881, %array_len1884
  %is_invalid1886 = or i1 %639, %640
  br i1 %is_invalid1886, label %assign_bounds.fail1887, label %assign_bounds.ok1888

assign_bounds.fail1887:                           ; preds = %bounds.ok1873
  %print_err1889 = call i32 (ptr, ...) @printf(ptr @err_msg.118)
  ret ptr null

assign_bounds.ok1888:                             ; preds = %bounds.ok1873
  %__where_src_0_load1890 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1891 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1890, i32 0, i32 1
  %data_header1892 = load ptr, ptr %df_data_ptr1891, align 8
  %641 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1892, i32 0, i32 2
  %642 = load ptr, ptr %641, align 8
  %643 = getelementptr ptr, ptr %642, i64 21
  %644 = load ptr, ptr %643, align 8
  %__where_i_0_load1893 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1894 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %644, i32 0, i32 0
  %data_field_ptr1895 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %644, i32 0, i32 2
  %data_ptr1896 = load ptr, ptr %data_field_ptr1895, align 8
  %array_len1897 = load i64, ptr %len_field_ptr1894, align 8
  %index_is_neg1898 = icmp slt i64 %__where_i_0_load1893, 0
  %index_rel1899 = add i64 %__where_i_0_load1893, %array_len1897
  %resolved_index1900 = select i1 %index_is_neg1898, i64 %index_rel1899, i64 %__where_i_0_load1893
  %is_neg1901 = icmp slt i64 %resolved_index1900, 0
  %is_too_big1902 = icmp sge i64 %resolved_index1900, %array_len1897
  %is_invalid1903 = or i1 %is_neg1901, %is_too_big1902
  br i1 %is_invalid1903, label %bounds.fail1904, label %bounds.ok1905

bounds.fail1904:                                  ; preds = %assign_bounds.ok1888
  %print_err1906 = call i32 (ptr, ...) @printf(ptr @err_msg.119)
  ret ptr null

bounds.ok1905:                                    ; preds = %assign_bounds.ok1888
  %elem_ptr1907 = getelementptr i8, ptr %data_ptr1896, i64 %resolved_index1900
  %loaded_val1908 = load i8, ptr %elem_ptr1907, align 1
  %elem_ptr1909 = getelementptr i8, ptr %data_ptr1885, i64 %__where_res_i_0_load1881
  store i8 %loaded_val1908, ptr %elem_ptr1909, align 1
  %__where_result_0_load1910 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1911 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1910, i32 0, i32 1
  %data_header1912 = load ptr, ptr %df_data_ptr1911, align 8
  %645 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1912, i32 0, i32 2
  %646 = load ptr, ptr %645, align 8
  %647 = getelementptr ptr, ptr %646, i64 22
  %648 = load ptr, ptr %647, align 8
  %__where_res_i_0_load1913 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1914 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %648, i32 0, i32 0
  %data_ptr_ptr1915 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %648, i32 0, i32 2
  %array_len1916 = load i64, ptr %len_ptr1914, align 4
  %data_ptr1917 = load ptr, ptr %data_ptr_ptr1915, align 8
  %649 = icmp slt i64 %__where_res_i_0_load1913, 0
  %650 = icmp sge i64 %__where_res_i_0_load1913, %array_len1916
  %is_invalid1918 = or i1 %649, %650
  br i1 %is_invalid1918, label %assign_bounds.fail1919, label %assign_bounds.ok1920

assign_bounds.fail1919:                           ; preds = %bounds.ok1905
  %print_err1921 = call i32 (ptr, ...) @printf(ptr @err_msg.120)
  ret ptr null

assign_bounds.ok1920:                             ; preds = %bounds.ok1905
  %__where_src_0_load1922 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1923 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1922, i32 0, i32 1
  %data_header1924 = load ptr, ptr %df_data_ptr1923, align 8
  %651 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1924, i32 0, i32 2
  %652 = load ptr, ptr %651, align 8
  %653 = getelementptr ptr, ptr %652, i64 22
  %654 = load ptr, ptr %653, align 8
  %__where_i_0_load1925 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1926 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %654, i32 0, i32 0
  %data_field_ptr1927 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %654, i32 0, i32 2
  %data_ptr1928 = load ptr, ptr %data_field_ptr1927, align 8
  %array_len1929 = load i64, ptr %len_field_ptr1926, align 8
  %index_is_neg1930 = icmp slt i64 %__where_i_0_load1925, 0
  %index_rel1931 = add i64 %__where_i_0_load1925, %array_len1929
  %resolved_index1932 = select i1 %index_is_neg1930, i64 %index_rel1931, i64 %__where_i_0_load1925
  %is_neg1933 = icmp slt i64 %resolved_index1932, 0
  %is_too_big1934 = icmp sge i64 %resolved_index1932, %array_len1929
  %is_invalid1935 = or i1 %is_neg1933, %is_too_big1934
  br i1 %is_invalid1935, label %bounds.fail1936, label %bounds.ok1937

bounds.fail1936:                                  ; preds = %assign_bounds.ok1920
  %print_err1938 = call i32 (ptr, ...) @printf(ptr @err_msg.121)
  ret ptr null

bounds.ok1937:                                    ; preds = %assign_bounds.ok1920
  %elem_ptr1939 = getelementptr i8, ptr %data_ptr1928, i64 %resolved_index1932
  %loaded_val1940 = load i8, ptr %elem_ptr1939, align 1
  %elem_ptr1941 = getelementptr i8, ptr %data_ptr1917, i64 %__where_res_i_0_load1913
  store i8 %loaded_val1940, ptr %elem_ptr1941, align 1
  %__where_result_0_load1942 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1943 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1942, i32 0, i32 1
  %data_header1944 = load ptr, ptr %df_data_ptr1943, align 8
  %655 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1944, i32 0, i32 2
  %656 = load ptr, ptr %655, align 8
  %657 = getelementptr ptr, ptr %656, i64 23
  %658 = load ptr, ptr %657, align 8
  %__where_res_i_0_load1945 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1946 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %658, i32 0, i32 0
  %data_ptr_ptr1947 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %658, i32 0, i32 2
  %array_len1948 = load i64, ptr %len_ptr1946, align 4
  %data_ptr1949 = load ptr, ptr %data_ptr_ptr1947, align 8
  %659 = icmp slt i64 %__where_res_i_0_load1945, 0
  %660 = icmp sge i64 %__where_res_i_0_load1945, %array_len1948
  %is_invalid1950 = or i1 %659, %660
  br i1 %is_invalid1950, label %assign_bounds.fail1951, label %assign_bounds.ok1952

assign_bounds.fail1951:                           ; preds = %bounds.ok1937
  %print_err1953 = call i32 (ptr, ...) @printf(ptr @err_msg.122)
  ret ptr null

assign_bounds.ok1952:                             ; preds = %bounds.ok1937
  %__where_src_0_load1954 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1955 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1954, i32 0, i32 1
  %data_header1956 = load ptr, ptr %df_data_ptr1955, align 8
  %661 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1956, i32 0, i32 2
  %662 = load ptr, ptr %661, align 8
  %663 = getelementptr ptr, ptr %662, i64 23
  %664 = load ptr, ptr %663, align 8
  %__where_i_0_load1957 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1958 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %664, i32 0, i32 0
  %data_field_ptr1959 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %664, i32 0, i32 2
  %data_ptr1960 = load ptr, ptr %data_field_ptr1959, align 8
  %array_len1961 = load i64, ptr %len_field_ptr1958, align 8
  %index_is_neg1962 = icmp slt i64 %__where_i_0_load1957, 0
  %index_rel1963 = add i64 %__where_i_0_load1957, %array_len1961
  %resolved_index1964 = select i1 %index_is_neg1962, i64 %index_rel1963, i64 %__where_i_0_load1957
  %is_neg1965 = icmp slt i64 %resolved_index1964, 0
  %is_too_big1966 = icmp sge i64 %resolved_index1964, %array_len1961
  %is_invalid1967 = or i1 %is_neg1965, %is_too_big1966
  br i1 %is_invalid1967, label %bounds.fail1968, label %bounds.ok1969

bounds.fail1968:                                  ; preds = %assign_bounds.ok1952
  %print_err1970 = call i32 (ptr, ...) @printf(ptr @err_msg.123)
  ret ptr null

bounds.ok1969:                                    ; preds = %assign_bounds.ok1952
  %elem_ptr1971 = getelementptr i8, ptr %data_ptr1960, i64 %resolved_index1964
  %loaded_val1972 = load i8, ptr %elem_ptr1971, align 1
  %elem_ptr1973 = getelementptr i8, ptr %data_ptr1949, i64 %__where_res_i_0_load1945
  store i8 %loaded_val1972, ptr %elem_ptr1973, align 1
  %__where_result_0_load1974 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr1975 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load1974, i32 0, i32 1
  %data_header1976 = load ptr, ptr %df_data_ptr1975, align 8
  %665 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1976, i32 0, i32 2
  %666 = load ptr, ptr %665, align 8
  %667 = getelementptr ptr, ptr %666, i64 24
  %668 = load ptr, ptr %667, align 8
  %__where_res_i_0_load1977 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr1978 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %668, i32 0, i32 0
  %data_ptr_ptr1979 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %668, i32 0, i32 2
  %array_len1980 = load i64, ptr %len_ptr1978, align 4
  %data_ptr1981 = load ptr, ptr %data_ptr_ptr1979, align 8
  %669 = icmp slt i64 %__where_res_i_0_load1977, 0
  %670 = icmp sge i64 %__where_res_i_0_load1977, %array_len1980
  %is_invalid1982 = or i1 %669, %670
  br i1 %is_invalid1982, label %assign_bounds.fail1983, label %assign_bounds.ok1984

assign_bounds.fail1983:                           ; preds = %bounds.ok1969
  %print_err1985 = call i32 (ptr, ...) @printf(ptr @err_msg.124)
  ret ptr null

assign_bounds.ok1984:                             ; preds = %bounds.ok1969
  %__where_src_0_load1986 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr1987 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load1986, i32 0, i32 1
  %data_header1988 = load ptr, ptr %df_data_ptr1987, align 8
  %671 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header1988, i32 0, i32 2
  %672 = load ptr, ptr %671, align 8
  %673 = getelementptr ptr, ptr %672, i64 24
  %674 = load ptr, ptr %673, align 8
  %__where_i_0_load1989 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr1990 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %674, i32 0, i32 0
  %data_field_ptr1991 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %674, i32 0, i32 2
  %data_ptr1992 = load ptr, ptr %data_field_ptr1991, align 8
  %array_len1993 = load i64, ptr %len_field_ptr1990, align 8
  %index_is_neg1994 = icmp slt i64 %__where_i_0_load1989, 0
  %index_rel1995 = add i64 %__where_i_0_load1989, %array_len1993
  %resolved_index1996 = select i1 %index_is_neg1994, i64 %index_rel1995, i64 %__where_i_0_load1989
  %is_neg1997 = icmp slt i64 %resolved_index1996, 0
  %is_too_big1998 = icmp sge i64 %resolved_index1996, %array_len1993
  %is_invalid1999 = or i1 %is_neg1997, %is_too_big1998
  br i1 %is_invalid1999, label %bounds.fail2000, label %bounds.ok2001

bounds.fail2000:                                  ; preds = %assign_bounds.ok1984
  %print_err2002 = call i32 (ptr, ...) @printf(ptr @err_msg.125)
  ret ptr null

bounds.ok2001:                                    ; preds = %assign_bounds.ok1984
  %elem_ptr2003 = getelementptr i8, ptr %data_ptr1992, i64 %resolved_index1996
  %loaded_val2004 = load i8, ptr %elem_ptr2003, align 1
  %elem_ptr2005 = getelementptr i8, ptr %data_ptr1981, i64 %__where_res_i_0_load1977
  store i8 %loaded_val2004, ptr %elem_ptr2005, align 1
  %__where_result_0_load2006 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2007 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2006, i32 0, i32 1
  %data_header2008 = load ptr, ptr %df_data_ptr2007, align 8
  %675 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2008, i32 0, i32 2
  %676 = load ptr, ptr %675, align 8
  %677 = getelementptr ptr, ptr %676, i64 25
  %678 = load ptr, ptr %677, align 8
  %__where_res_i_0_load2009 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2010 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %678, i32 0, i32 0
  %data_ptr_ptr2011 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %678, i32 0, i32 2
  %array_len2012 = load i64, ptr %len_ptr2010, align 4
  %data_ptr2013 = load ptr, ptr %data_ptr_ptr2011, align 8
  %679 = icmp slt i64 %__where_res_i_0_load2009, 0
  %680 = icmp sge i64 %__where_res_i_0_load2009, %array_len2012
  %is_invalid2014 = or i1 %679, %680
  br i1 %is_invalid2014, label %assign_bounds.fail2015, label %assign_bounds.ok2016

assign_bounds.fail2015:                           ; preds = %bounds.ok2001
  %print_err2017 = call i32 (ptr, ...) @printf(ptr @err_msg.126)
  ret ptr null

assign_bounds.ok2016:                             ; preds = %bounds.ok2001
  %__where_src_0_load2018 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2019 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2018, i32 0, i32 1
  %data_header2020 = load ptr, ptr %df_data_ptr2019, align 8
  %681 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2020, i32 0, i32 2
  %682 = load ptr, ptr %681, align 8
  %683 = getelementptr ptr, ptr %682, i64 25
  %684 = load ptr, ptr %683, align 8
  %__where_i_0_load2021 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2022 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %684, i32 0, i32 0
  %data_field_ptr2023 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %684, i32 0, i32 2
  %data_ptr2024 = load ptr, ptr %data_field_ptr2023, align 8
  %array_len2025 = load i64, ptr %len_field_ptr2022, align 8
  %index_is_neg2026 = icmp slt i64 %__where_i_0_load2021, 0
  %index_rel2027 = add i64 %__where_i_0_load2021, %array_len2025
  %resolved_index2028 = select i1 %index_is_neg2026, i64 %index_rel2027, i64 %__where_i_0_load2021
  %is_neg2029 = icmp slt i64 %resolved_index2028, 0
  %is_too_big2030 = icmp sge i64 %resolved_index2028, %array_len2025
  %is_invalid2031 = or i1 %is_neg2029, %is_too_big2030
  br i1 %is_invalid2031, label %bounds.fail2032, label %bounds.ok2033

bounds.fail2032:                                  ; preds = %assign_bounds.ok2016
  %print_err2034 = call i32 (ptr, ...) @printf(ptr @err_msg.127)
  ret ptr null

bounds.ok2033:                                    ; preds = %assign_bounds.ok2016
  %elem_ptr2035 = getelementptr i8, ptr %data_ptr2024, i64 %resolved_index2028
  %loaded_val2036 = load i8, ptr %elem_ptr2035, align 1
  %elem_ptr2037 = getelementptr i8, ptr %data_ptr2013, i64 %__where_res_i_0_load2009
  store i8 %loaded_val2036, ptr %elem_ptr2037, align 1
  %__where_result_0_load2038 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2039 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2038, i32 0, i32 1
  %data_header2040 = load ptr, ptr %df_data_ptr2039, align 8
  %685 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2040, i32 0, i32 2
  %686 = load ptr, ptr %685, align 8
  %687 = getelementptr ptr, ptr %686, i64 26
  %688 = load ptr, ptr %687, align 8
  %__where_res_i_0_load2041 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2042 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %688, i32 0, i32 0
  %data_ptr_ptr2043 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %688, i32 0, i32 2
  %array_len2044 = load i64, ptr %len_ptr2042, align 4
  %data_ptr2045 = load ptr, ptr %data_ptr_ptr2043, align 8
  %689 = icmp slt i64 %__where_res_i_0_load2041, 0
  %690 = icmp sge i64 %__where_res_i_0_load2041, %array_len2044
  %is_invalid2046 = or i1 %689, %690
  br i1 %is_invalid2046, label %assign_bounds.fail2047, label %assign_bounds.ok2048

assign_bounds.fail2047:                           ; preds = %bounds.ok2033
  %print_err2049 = call i32 (ptr, ...) @printf(ptr @err_msg.128)
  ret ptr null

assign_bounds.ok2048:                             ; preds = %bounds.ok2033
  %__where_src_0_load2050 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2051 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2050, i32 0, i32 1
  %data_header2052 = load ptr, ptr %df_data_ptr2051, align 8
  %691 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2052, i32 0, i32 2
  %692 = load ptr, ptr %691, align 8
  %693 = getelementptr ptr, ptr %692, i64 26
  %694 = load ptr, ptr %693, align 8
  %__where_i_0_load2053 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2054 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %694, i32 0, i32 0
  %data_field_ptr2055 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %694, i32 0, i32 2
  %data_ptr2056 = load ptr, ptr %data_field_ptr2055, align 8
  %array_len2057 = load i64, ptr %len_field_ptr2054, align 8
  %index_is_neg2058 = icmp slt i64 %__where_i_0_load2053, 0
  %index_rel2059 = add i64 %__where_i_0_load2053, %array_len2057
  %resolved_index2060 = select i1 %index_is_neg2058, i64 %index_rel2059, i64 %__where_i_0_load2053
  %is_neg2061 = icmp slt i64 %resolved_index2060, 0
  %is_too_big2062 = icmp sge i64 %resolved_index2060, %array_len2057
  %is_invalid2063 = or i1 %is_neg2061, %is_too_big2062
  br i1 %is_invalid2063, label %bounds.fail2064, label %bounds.ok2065

bounds.fail2064:                                  ; preds = %assign_bounds.ok2048
  %print_err2066 = call i32 (ptr, ...) @printf(ptr @err_msg.129)
  ret ptr null

bounds.ok2065:                                    ; preds = %assign_bounds.ok2048
  %elem_ptr2067 = getelementptr i8, ptr %data_ptr2056, i64 %resolved_index2060
  %loaded_val2068 = load i8, ptr %elem_ptr2067, align 1
  %elem_ptr2069 = getelementptr i8, ptr %data_ptr2045, i64 %__where_res_i_0_load2041
  store i8 %loaded_val2068, ptr %elem_ptr2069, align 1
  %__where_result_0_load2070 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2071 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2070, i32 0, i32 1
  %data_header2072 = load ptr, ptr %df_data_ptr2071, align 8
  %695 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2072, i32 0, i32 2
  %696 = load ptr, ptr %695, align 8
  %697 = getelementptr ptr, ptr %696, i64 27
  %698 = load ptr, ptr %697, align 8
  %__where_res_i_0_load2073 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2074 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %698, i32 0, i32 0
  %data_ptr_ptr2075 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %698, i32 0, i32 2
  %array_len2076 = load i64, ptr %len_ptr2074, align 4
  %data_ptr2077 = load ptr, ptr %data_ptr_ptr2075, align 8
  %699 = icmp slt i64 %__where_res_i_0_load2073, 0
  %700 = icmp sge i64 %__where_res_i_0_load2073, %array_len2076
  %is_invalid2078 = or i1 %699, %700
  br i1 %is_invalid2078, label %assign_bounds.fail2079, label %assign_bounds.ok2080

assign_bounds.fail2079:                           ; preds = %bounds.ok2065
  %print_err2081 = call i32 (ptr, ...) @printf(ptr @err_msg.130)
  ret ptr null

assign_bounds.ok2080:                             ; preds = %bounds.ok2065
  %__where_src_0_load2082 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2083 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2082, i32 0, i32 1
  %data_header2084 = load ptr, ptr %df_data_ptr2083, align 8
  %701 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2084, i32 0, i32 2
  %702 = load ptr, ptr %701, align 8
  %703 = getelementptr ptr, ptr %702, i64 27
  %704 = load ptr, ptr %703, align 8
  %__where_i_0_load2085 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2086 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %704, i32 0, i32 0
  %data_field_ptr2087 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %704, i32 0, i32 2
  %data_ptr2088 = load ptr, ptr %data_field_ptr2087, align 8
  %array_len2089 = load i64, ptr %len_field_ptr2086, align 8
  %index_is_neg2090 = icmp slt i64 %__where_i_0_load2085, 0
  %index_rel2091 = add i64 %__where_i_0_load2085, %array_len2089
  %resolved_index2092 = select i1 %index_is_neg2090, i64 %index_rel2091, i64 %__where_i_0_load2085
  %is_neg2093 = icmp slt i64 %resolved_index2092, 0
  %is_too_big2094 = icmp sge i64 %resolved_index2092, %array_len2089
  %is_invalid2095 = or i1 %is_neg2093, %is_too_big2094
  br i1 %is_invalid2095, label %bounds.fail2096, label %bounds.ok2097

bounds.fail2096:                                  ; preds = %assign_bounds.ok2080
  %print_err2098 = call i32 (ptr, ...) @printf(ptr @err_msg.131)
  ret ptr null

bounds.ok2097:                                    ; preds = %assign_bounds.ok2080
  %elem_ptr2099 = getelementptr i8, ptr %data_ptr2088, i64 %resolved_index2092
  %loaded_val2100 = load i8, ptr %elem_ptr2099, align 1
  %elem_ptr2101 = getelementptr i8, ptr %data_ptr2077, i64 %__where_res_i_0_load2073
  store i8 %loaded_val2100, ptr %elem_ptr2101, align 1
  %__where_result_0_load2102 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2103 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2102, i32 0, i32 1
  %data_header2104 = load ptr, ptr %df_data_ptr2103, align 8
  %705 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2104, i32 0, i32 2
  %706 = load ptr, ptr %705, align 8
  %707 = getelementptr ptr, ptr %706, i64 28
  %708 = load ptr, ptr %707, align 8
  %__where_res_i_0_load2105 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2106 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %708, i32 0, i32 0
  %data_ptr_ptr2107 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %708, i32 0, i32 2
  %array_len2108 = load i64, ptr %len_ptr2106, align 4
  %data_ptr2109 = load ptr, ptr %data_ptr_ptr2107, align 8
  %709 = icmp slt i64 %__where_res_i_0_load2105, 0
  %710 = icmp sge i64 %__where_res_i_0_load2105, %array_len2108
  %is_invalid2110 = or i1 %709, %710
  br i1 %is_invalid2110, label %assign_bounds.fail2111, label %assign_bounds.ok2112

assign_bounds.fail2111:                           ; preds = %bounds.ok2097
  %print_err2113 = call i32 (ptr, ...) @printf(ptr @err_msg.132)
  ret ptr null

assign_bounds.ok2112:                             ; preds = %bounds.ok2097
  %__where_src_0_load2114 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2115 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2114, i32 0, i32 1
  %data_header2116 = load ptr, ptr %df_data_ptr2115, align 8
  %711 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2116, i32 0, i32 2
  %712 = load ptr, ptr %711, align 8
  %713 = getelementptr ptr, ptr %712, i64 28
  %714 = load ptr, ptr %713, align 8
  %__where_i_0_load2117 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2118 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %714, i32 0, i32 0
  %data_field_ptr2119 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %714, i32 0, i32 2
  %data_ptr2120 = load ptr, ptr %data_field_ptr2119, align 8
  %array_len2121 = load i64, ptr %len_field_ptr2118, align 8
  %index_is_neg2122 = icmp slt i64 %__where_i_0_load2117, 0
  %index_rel2123 = add i64 %__where_i_0_load2117, %array_len2121
  %resolved_index2124 = select i1 %index_is_neg2122, i64 %index_rel2123, i64 %__where_i_0_load2117
  %is_neg2125 = icmp slt i64 %resolved_index2124, 0
  %is_too_big2126 = icmp sge i64 %resolved_index2124, %array_len2121
  %is_invalid2127 = or i1 %is_neg2125, %is_too_big2126
  br i1 %is_invalid2127, label %bounds.fail2128, label %bounds.ok2129

bounds.fail2128:                                  ; preds = %assign_bounds.ok2112
  %print_err2130 = call i32 (ptr, ...) @printf(ptr @err_msg.133)
  ret ptr null

bounds.ok2129:                                    ; preds = %assign_bounds.ok2112
  %elem_ptr2131 = getelementptr i8, ptr %data_ptr2120, i64 %resolved_index2124
  %loaded_val2132 = load i8, ptr %elem_ptr2131, align 1
  %elem_ptr2133 = getelementptr i8, ptr %data_ptr2109, i64 %__where_res_i_0_load2105
  store i8 %loaded_val2132, ptr %elem_ptr2133, align 1
  %__where_result_0_load2134 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2135 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2134, i32 0, i32 1
  %data_header2136 = load ptr, ptr %df_data_ptr2135, align 8
  %715 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2136, i32 0, i32 2
  %716 = load ptr, ptr %715, align 8
  %717 = getelementptr ptr, ptr %716, i64 29
  %718 = load ptr, ptr %717, align 8
  %__where_res_i_0_load2137 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2138 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %718, i32 0, i32 0
  %data_ptr_ptr2139 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %718, i32 0, i32 2
  %array_len2140 = load i64, ptr %len_ptr2138, align 4
  %data_ptr2141 = load ptr, ptr %data_ptr_ptr2139, align 8
  %719 = icmp slt i64 %__where_res_i_0_load2137, 0
  %720 = icmp sge i64 %__where_res_i_0_load2137, %array_len2140
  %is_invalid2142 = or i1 %719, %720
  br i1 %is_invalid2142, label %assign_bounds.fail2143, label %assign_bounds.ok2144

assign_bounds.fail2143:                           ; preds = %bounds.ok2129
  %print_err2145 = call i32 (ptr, ...) @printf(ptr @err_msg.134)
  ret ptr null

assign_bounds.ok2144:                             ; preds = %bounds.ok2129
  %__where_src_0_load2146 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2147 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2146, i32 0, i32 1
  %data_header2148 = load ptr, ptr %df_data_ptr2147, align 8
  %721 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2148, i32 0, i32 2
  %722 = load ptr, ptr %721, align 8
  %723 = getelementptr ptr, ptr %722, i64 29
  %724 = load ptr, ptr %723, align 8
  %__where_i_0_load2149 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2150 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %724, i32 0, i32 0
  %data_field_ptr2151 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %724, i32 0, i32 2
  %data_ptr2152 = load ptr, ptr %data_field_ptr2151, align 8
  %array_len2153 = load i64, ptr %len_field_ptr2150, align 8
  %index_is_neg2154 = icmp slt i64 %__where_i_0_load2149, 0
  %index_rel2155 = add i64 %__where_i_0_load2149, %array_len2153
  %resolved_index2156 = select i1 %index_is_neg2154, i64 %index_rel2155, i64 %__where_i_0_load2149
  %is_neg2157 = icmp slt i64 %resolved_index2156, 0
  %is_too_big2158 = icmp sge i64 %resolved_index2156, %array_len2153
  %is_invalid2159 = or i1 %is_neg2157, %is_too_big2158
  br i1 %is_invalid2159, label %bounds.fail2160, label %bounds.ok2161

bounds.fail2160:                                  ; preds = %assign_bounds.ok2144
  %print_err2162 = call i32 (ptr, ...) @printf(ptr @err_msg.135)
  ret ptr null

bounds.ok2161:                                    ; preds = %assign_bounds.ok2144
  %elem_ptr2163 = getelementptr i8, ptr %data_ptr2152, i64 %resolved_index2156
  %loaded_val2164 = load i8, ptr %elem_ptr2163, align 1
  %elem_ptr2165 = getelementptr i8, ptr %data_ptr2141, i64 %__where_res_i_0_load2137
  store i8 %loaded_val2164, ptr %elem_ptr2165, align 1
  %__where_result_0_load2166 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2167 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2166, i32 0, i32 1
  %data_header2168 = load ptr, ptr %df_data_ptr2167, align 8
  %725 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2168, i32 0, i32 2
  %726 = load ptr, ptr %725, align 8
  %727 = getelementptr ptr, ptr %726, i64 30
  %728 = load ptr, ptr %727, align 8
  %__where_res_i_0_load2169 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2170 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %728, i32 0, i32 0
  %data_ptr_ptr2171 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %728, i32 0, i32 2
  %array_len2172 = load i64, ptr %len_ptr2170, align 4
  %data_ptr2173 = load ptr, ptr %data_ptr_ptr2171, align 8
  %729 = icmp slt i64 %__where_res_i_0_load2169, 0
  %730 = icmp sge i64 %__where_res_i_0_load2169, %array_len2172
  %is_invalid2174 = or i1 %729, %730
  br i1 %is_invalid2174, label %assign_bounds.fail2175, label %assign_bounds.ok2176

assign_bounds.fail2175:                           ; preds = %bounds.ok2161
  %print_err2177 = call i32 (ptr, ...) @printf(ptr @err_msg.136)
  ret ptr null

assign_bounds.ok2176:                             ; preds = %bounds.ok2161
  %__where_src_0_load2178 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2179 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2178, i32 0, i32 1
  %data_header2180 = load ptr, ptr %df_data_ptr2179, align 8
  %731 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2180, i32 0, i32 2
  %732 = load ptr, ptr %731, align 8
  %733 = getelementptr ptr, ptr %732, i64 30
  %734 = load ptr, ptr %733, align 8
  %__where_i_0_load2181 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2182 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %734, i32 0, i32 0
  %data_field_ptr2183 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %734, i32 0, i32 2
  %data_ptr2184 = load ptr, ptr %data_field_ptr2183, align 8
  %array_len2185 = load i64, ptr %len_field_ptr2182, align 8
  %index_is_neg2186 = icmp slt i64 %__where_i_0_load2181, 0
  %index_rel2187 = add i64 %__where_i_0_load2181, %array_len2185
  %resolved_index2188 = select i1 %index_is_neg2186, i64 %index_rel2187, i64 %__where_i_0_load2181
  %is_neg2189 = icmp slt i64 %resolved_index2188, 0
  %is_too_big2190 = icmp sge i64 %resolved_index2188, %array_len2185
  %is_invalid2191 = or i1 %is_neg2189, %is_too_big2190
  br i1 %is_invalid2191, label %bounds.fail2192, label %bounds.ok2193

bounds.fail2192:                                  ; preds = %assign_bounds.ok2176
  %print_err2194 = call i32 (ptr, ...) @printf(ptr @err_msg.137)
  ret ptr null

bounds.ok2193:                                    ; preds = %assign_bounds.ok2176
  %elem_ptr2195 = getelementptr i8, ptr %data_ptr2184, i64 %resolved_index2188
  %loaded_val2196 = load i8, ptr %elem_ptr2195, align 1
  %elem_ptr2197 = getelementptr i8, ptr %data_ptr2173, i64 %__where_res_i_0_load2169
  store i8 %loaded_val2196, ptr %elem_ptr2197, align 1
  %__where_result_0_load2198 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2199 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2198, i32 0, i32 1
  %data_header2200 = load ptr, ptr %df_data_ptr2199, align 8
  %735 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2200, i32 0, i32 2
  %736 = load ptr, ptr %735, align 8
  %737 = getelementptr ptr, ptr %736, i64 31
  %738 = load ptr, ptr %737, align 8
  %__where_res_i_0_load2201 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2202 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %738, i32 0, i32 0
  %data_ptr_ptr2203 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %738, i32 0, i32 2
  %array_len2204 = load i64, ptr %len_ptr2202, align 4
  %data_ptr2205 = load ptr, ptr %data_ptr_ptr2203, align 8
  %739 = icmp slt i64 %__where_res_i_0_load2201, 0
  %740 = icmp sge i64 %__where_res_i_0_load2201, %array_len2204
  %is_invalid2206 = or i1 %739, %740
  br i1 %is_invalid2206, label %assign_bounds.fail2207, label %assign_bounds.ok2208

assign_bounds.fail2207:                           ; preds = %bounds.ok2193
  %print_err2209 = call i32 (ptr, ...) @printf(ptr @err_msg.138)
  ret ptr null

assign_bounds.ok2208:                             ; preds = %bounds.ok2193
  %__where_src_0_load2210 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2211 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2210, i32 0, i32 1
  %data_header2212 = load ptr, ptr %df_data_ptr2211, align 8
  %741 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2212, i32 0, i32 2
  %742 = load ptr, ptr %741, align 8
  %743 = getelementptr ptr, ptr %742, i64 31
  %744 = load ptr, ptr %743, align 8
  %__where_i_0_load2213 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2214 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %744, i32 0, i32 0
  %data_field_ptr2215 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %744, i32 0, i32 2
  %data_ptr2216 = load ptr, ptr %data_field_ptr2215, align 8
  %array_len2217 = load i64, ptr %len_field_ptr2214, align 8
  %index_is_neg2218 = icmp slt i64 %__where_i_0_load2213, 0
  %index_rel2219 = add i64 %__where_i_0_load2213, %array_len2217
  %resolved_index2220 = select i1 %index_is_neg2218, i64 %index_rel2219, i64 %__where_i_0_load2213
  %is_neg2221 = icmp slt i64 %resolved_index2220, 0
  %is_too_big2222 = icmp sge i64 %resolved_index2220, %array_len2217
  %is_invalid2223 = or i1 %is_neg2221, %is_too_big2222
  br i1 %is_invalid2223, label %bounds.fail2224, label %bounds.ok2225

bounds.fail2224:                                  ; preds = %assign_bounds.ok2208
  %print_err2226 = call i32 (ptr, ...) @printf(ptr @err_msg.139)
  ret ptr null

bounds.ok2225:                                    ; preds = %assign_bounds.ok2208
  %elem_ptr2227 = getelementptr i8, ptr %data_ptr2216, i64 %resolved_index2220
  %loaded_val2228 = load i8, ptr %elem_ptr2227, align 1
  %elem_ptr2229 = getelementptr i8, ptr %data_ptr2205, i64 %__where_res_i_0_load2201
  store i8 %loaded_val2228, ptr %elem_ptr2229, align 1
  %__where_result_0_load2230 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2231 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2230, i32 0, i32 1
  %data_header2232 = load ptr, ptr %df_data_ptr2231, align 8
  %745 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2232, i32 0, i32 2
  %746 = load ptr, ptr %745, align 8
  %747 = getelementptr ptr, ptr %746, i64 32
  %748 = load ptr, ptr %747, align 8
  %__where_res_i_0_load2233 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2234 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %748, i32 0, i32 0
  %data_ptr_ptr2235 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %748, i32 0, i32 2
  %array_len2236 = load i64, ptr %len_ptr2234, align 4
  %data_ptr2237 = load ptr, ptr %data_ptr_ptr2235, align 8
  %749 = icmp slt i64 %__where_res_i_0_load2233, 0
  %750 = icmp sge i64 %__where_res_i_0_load2233, %array_len2236
  %is_invalid2238 = or i1 %749, %750
  br i1 %is_invalid2238, label %assign_bounds.fail2239, label %assign_bounds.ok2240

assign_bounds.fail2239:                           ; preds = %bounds.ok2225
  %print_err2241 = call i32 (ptr, ...) @printf(ptr @err_msg.140)
  ret ptr null

assign_bounds.ok2240:                             ; preds = %bounds.ok2225
  %__where_src_0_load2242 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2243 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2242, i32 0, i32 1
  %data_header2244 = load ptr, ptr %df_data_ptr2243, align 8
  %751 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2244, i32 0, i32 2
  %752 = load ptr, ptr %751, align 8
  %753 = getelementptr ptr, ptr %752, i64 32
  %754 = load ptr, ptr %753, align 8
  %__where_i_0_load2245 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2246 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %754, i32 0, i32 0
  %data_field_ptr2247 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %754, i32 0, i32 2
  %data_ptr2248 = load ptr, ptr %data_field_ptr2247, align 8
  %array_len2249 = load i64, ptr %len_field_ptr2246, align 8
  %index_is_neg2250 = icmp slt i64 %__where_i_0_load2245, 0
  %index_rel2251 = add i64 %__where_i_0_load2245, %array_len2249
  %resolved_index2252 = select i1 %index_is_neg2250, i64 %index_rel2251, i64 %__where_i_0_load2245
  %is_neg2253 = icmp slt i64 %resolved_index2252, 0
  %is_too_big2254 = icmp sge i64 %resolved_index2252, %array_len2249
  %is_invalid2255 = or i1 %is_neg2253, %is_too_big2254
  br i1 %is_invalid2255, label %bounds.fail2256, label %bounds.ok2257

bounds.fail2256:                                  ; preds = %assign_bounds.ok2240
  %print_err2258 = call i32 (ptr, ...) @printf(ptr @err_msg.141)
  ret ptr null

bounds.ok2257:                                    ; preds = %assign_bounds.ok2240
  %elem_ptr2259 = getelementptr i8, ptr %data_ptr2248, i64 %resolved_index2252
  %loaded_val2260 = load i8, ptr %elem_ptr2259, align 1
  %elem_ptr2261 = getelementptr i8, ptr %data_ptr2237, i64 %__where_res_i_0_load2233
  store i8 %loaded_val2260, ptr %elem_ptr2261, align 1
  %__where_result_0_load2262 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2263 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2262, i32 0, i32 1
  %data_header2264 = load ptr, ptr %df_data_ptr2263, align 8
  %755 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2264, i32 0, i32 2
  %756 = load ptr, ptr %755, align 8
  %757 = getelementptr ptr, ptr %756, i64 33
  %758 = load ptr, ptr %757, align 8
  %__where_res_i_0_load2265 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2266 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %758, i32 0, i32 0
  %data_ptr_ptr2267 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %758, i32 0, i32 2
  %array_len2268 = load i64, ptr %len_ptr2266, align 4
  %data_ptr2269 = load ptr, ptr %data_ptr_ptr2267, align 8
  %759 = icmp slt i64 %__where_res_i_0_load2265, 0
  %760 = icmp sge i64 %__where_res_i_0_load2265, %array_len2268
  %is_invalid2270 = or i1 %759, %760
  br i1 %is_invalid2270, label %assign_bounds.fail2271, label %assign_bounds.ok2272

assign_bounds.fail2271:                           ; preds = %bounds.ok2257
  %print_err2273 = call i32 (ptr, ...) @printf(ptr @err_msg.142)
  ret ptr null

assign_bounds.ok2272:                             ; preds = %bounds.ok2257
  %__where_src_0_load2274 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2275 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2274, i32 0, i32 1
  %data_header2276 = load ptr, ptr %df_data_ptr2275, align 8
  %761 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2276, i32 0, i32 2
  %762 = load ptr, ptr %761, align 8
  %763 = getelementptr ptr, ptr %762, i64 33
  %764 = load ptr, ptr %763, align 8
  %__where_i_0_load2277 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2278 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %764, i32 0, i32 0
  %data_field_ptr2279 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %764, i32 0, i32 2
  %data_ptr2280 = load ptr, ptr %data_field_ptr2279, align 8
  %array_len2281 = load i64, ptr %len_field_ptr2278, align 8
  %index_is_neg2282 = icmp slt i64 %__where_i_0_load2277, 0
  %index_rel2283 = add i64 %__where_i_0_load2277, %array_len2281
  %resolved_index2284 = select i1 %index_is_neg2282, i64 %index_rel2283, i64 %__where_i_0_load2277
  %is_neg2285 = icmp slt i64 %resolved_index2284, 0
  %is_too_big2286 = icmp sge i64 %resolved_index2284, %array_len2281
  %is_invalid2287 = or i1 %is_neg2285, %is_too_big2286
  br i1 %is_invalid2287, label %bounds.fail2288, label %bounds.ok2289

bounds.fail2288:                                  ; preds = %assign_bounds.ok2272
  %print_err2290 = call i32 (ptr, ...) @printf(ptr @err_msg.143)
  ret ptr null

bounds.ok2289:                                    ; preds = %assign_bounds.ok2272
  %elem_ptr2291 = getelementptr i8, ptr %data_ptr2280, i64 %resolved_index2284
  %loaded_val2292 = load i8, ptr %elem_ptr2291, align 1
  %elem_ptr2293 = getelementptr i8, ptr %data_ptr2269, i64 %__where_res_i_0_load2265
  store i8 %loaded_val2292, ptr %elem_ptr2293, align 1
  %__where_result_0_load2294 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2295 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2294, i32 0, i32 1
  %data_header2296 = load ptr, ptr %df_data_ptr2295, align 8
  %765 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2296, i32 0, i32 2
  %766 = load ptr, ptr %765, align 8
  %767 = getelementptr ptr, ptr %766, i64 34
  %768 = load ptr, ptr %767, align 8
  %__where_res_i_0_load2297 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2298 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %768, i32 0, i32 0
  %data_ptr_ptr2299 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %768, i32 0, i32 2
  %array_len2300 = load i64, ptr %len_ptr2298, align 4
  %data_ptr2301 = load ptr, ptr %data_ptr_ptr2299, align 8
  %769 = icmp slt i64 %__where_res_i_0_load2297, 0
  %770 = icmp sge i64 %__where_res_i_0_load2297, %array_len2300
  %is_invalid2302 = or i1 %769, %770
  br i1 %is_invalid2302, label %assign_bounds.fail2303, label %assign_bounds.ok2304

assign_bounds.fail2303:                           ; preds = %bounds.ok2289
  %print_err2305 = call i32 (ptr, ...) @printf(ptr @err_msg.144)
  ret ptr null

assign_bounds.ok2304:                             ; preds = %bounds.ok2289
  %__where_src_0_load2306 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2307 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2306, i32 0, i32 1
  %data_header2308 = load ptr, ptr %df_data_ptr2307, align 8
  %771 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2308, i32 0, i32 2
  %772 = load ptr, ptr %771, align 8
  %773 = getelementptr ptr, ptr %772, i64 34
  %774 = load ptr, ptr %773, align 8
  %__where_i_0_load2309 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2310 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %774, i32 0, i32 0
  %data_field_ptr2311 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %774, i32 0, i32 2
  %data_ptr2312 = load ptr, ptr %data_field_ptr2311, align 8
  %array_len2313 = load i64, ptr %len_field_ptr2310, align 8
  %index_is_neg2314 = icmp slt i64 %__where_i_0_load2309, 0
  %index_rel2315 = add i64 %__where_i_0_load2309, %array_len2313
  %resolved_index2316 = select i1 %index_is_neg2314, i64 %index_rel2315, i64 %__where_i_0_load2309
  %is_neg2317 = icmp slt i64 %resolved_index2316, 0
  %is_too_big2318 = icmp sge i64 %resolved_index2316, %array_len2313
  %is_invalid2319 = or i1 %is_neg2317, %is_too_big2318
  br i1 %is_invalid2319, label %bounds.fail2320, label %bounds.ok2321

bounds.fail2320:                                  ; preds = %assign_bounds.ok2304
  %print_err2322 = call i32 (ptr, ...) @printf(ptr @err_msg.145)
  ret ptr null

bounds.ok2321:                                    ; preds = %assign_bounds.ok2304
  %elem_ptr2323 = getelementptr i8, ptr %data_ptr2312, i64 %resolved_index2316
  %loaded_val2324 = load i8, ptr %elem_ptr2323, align 1
  %elem_ptr2325 = getelementptr i8, ptr %data_ptr2301, i64 %__where_res_i_0_load2297
  store i8 %loaded_val2324, ptr %elem_ptr2325, align 1
  %__where_result_0_load2326 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2327 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2326, i32 0, i32 1
  %data_header2328 = load ptr, ptr %df_data_ptr2327, align 8
  %775 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2328, i32 0, i32 2
  %776 = load ptr, ptr %775, align 8
  %777 = getelementptr ptr, ptr %776, i64 35
  %778 = load ptr, ptr %777, align 8
  %__where_res_i_0_load2329 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2330 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %778, i32 0, i32 0
  %data_ptr_ptr2331 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %778, i32 0, i32 2
  %array_len2332 = load i64, ptr %len_ptr2330, align 4
  %data_ptr2333 = load ptr, ptr %data_ptr_ptr2331, align 8
  %779 = icmp slt i64 %__where_res_i_0_load2329, 0
  %780 = icmp sge i64 %__where_res_i_0_load2329, %array_len2332
  %is_invalid2334 = or i1 %779, %780
  br i1 %is_invalid2334, label %assign_bounds.fail2335, label %assign_bounds.ok2336

assign_bounds.fail2335:                           ; preds = %bounds.ok2321
  %print_err2337 = call i32 (ptr, ...) @printf(ptr @err_msg.146)
  ret ptr null

assign_bounds.ok2336:                             ; preds = %bounds.ok2321
  %__where_src_0_load2338 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2339 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2338, i32 0, i32 1
  %data_header2340 = load ptr, ptr %df_data_ptr2339, align 8
  %781 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2340, i32 0, i32 2
  %782 = load ptr, ptr %781, align 8
  %783 = getelementptr ptr, ptr %782, i64 35
  %784 = load ptr, ptr %783, align 8
  %__where_i_0_load2341 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2342 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %784, i32 0, i32 0
  %data_field_ptr2343 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %784, i32 0, i32 2
  %data_ptr2344 = load ptr, ptr %data_field_ptr2343, align 8
  %array_len2345 = load i64, ptr %len_field_ptr2342, align 8
  %index_is_neg2346 = icmp slt i64 %__where_i_0_load2341, 0
  %index_rel2347 = add i64 %__where_i_0_load2341, %array_len2345
  %resolved_index2348 = select i1 %index_is_neg2346, i64 %index_rel2347, i64 %__where_i_0_load2341
  %is_neg2349 = icmp slt i64 %resolved_index2348, 0
  %is_too_big2350 = icmp sge i64 %resolved_index2348, %array_len2345
  %is_invalid2351 = or i1 %is_neg2349, %is_too_big2350
  br i1 %is_invalid2351, label %bounds.fail2352, label %bounds.ok2353

bounds.fail2352:                                  ; preds = %assign_bounds.ok2336
  %print_err2354 = call i32 (ptr, ...) @printf(ptr @err_msg.147)
  ret ptr null

bounds.ok2353:                                    ; preds = %assign_bounds.ok2336
  %elem_ptr2355 = getelementptr i8, ptr %data_ptr2344, i64 %resolved_index2348
  %loaded_val2356 = load i8, ptr %elem_ptr2355, align 1
  %elem_ptr2357 = getelementptr i8, ptr %data_ptr2333, i64 %__where_res_i_0_load2329
  store i8 %loaded_val2356, ptr %elem_ptr2357, align 1
  %__where_result_0_load2358 = load ptr, ptr @__where_result_0, align 8
  %df_data_ptr2359 = getelementptr inbounds nuw %dataframe, ptr %__where_result_0_load2358, i32 0, i32 1
  %data_header2360 = load ptr, ptr %df_data_ptr2359, align 8
  %785 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2360, i32 0, i32 2
  %786 = load ptr, ptr %785, align 8
  %787 = getelementptr ptr, ptr %786, i64 36
  %788 = load ptr, ptr %787, align 8
  %__where_res_i_0_load2361 = load i64, ptr @__where_res_i_0, align 8
  %len_ptr2362 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %788, i32 0, i32 0
  %data_ptr_ptr2363 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %788, i32 0, i32 2
  %array_len2364 = load i64, ptr %len_ptr2362, align 4
  %data_ptr2365 = load ptr, ptr %data_ptr_ptr2363, align 8
  %789 = icmp slt i64 %__where_res_i_0_load2361, 0
  %790 = icmp sge i64 %__where_res_i_0_load2361, %array_len2364
  %is_invalid2366 = or i1 %789, %790
  br i1 %is_invalid2366, label %assign_bounds.fail2367, label %assign_bounds.ok2368

assign_bounds.fail2367:                           ; preds = %bounds.ok2353
  %print_err2369 = call i32 (ptr, ...) @printf(ptr @err_msg.148)
  ret ptr null

assign_bounds.ok2368:                             ; preds = %bounds.ok2353
  %__where_src_0_load2370 = load ptr, ptr @__where_src_0, align 8
  %df_data_ptr2371 = getelementptr inbounds nuw %dataframe, ptr %__where_src_0_load2370, i32 0, i32 1
  %data_header2372 = load ptr, ptr %df_data_ptr2371, align 8
  %791 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header2372, i32 0, i32 2
  %792 = load ptr, ptr %791, align 8
  %793 = getelementptr ptr, ptr %792, i64 36
  %794 = load ptr, ptr %793, align 8
  %__where_i_0_load2373 = load i64, ptr @__where_i_0, align 8
  %len_field_ptr2374 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %794, i32 0, i32 0
  %data_field_ptr2375 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %794, i32 0, i32 2
  %data_ptr2376 = load ptr, ptr %data_field_ptr2375, align 8
  %array_len2377 = load i64, ptr %len_field_ptr2374, align 8
  %index_is_neg2378 = icmp slt i64 %__where_i_0_load2373, 0
  %index_rel2379 = add i64 %__where_i_0_load2373, %array_len2377
  %resolved_index2380 = select i1 %index_is_neg2378, i64 %index_rel2379, i64 %__where_i_0_load2373
  %is_neg2381 = icmp slt i64 %resolved_index2380, 0
  %is_too_big2382 = icmp sge i64 %resolved_index2380, %array_len2377
  %is_invalid2383 = or i1 %is_neg2381, %is_too_big2382
  br i1 %is_invalid2383, label %bounds.fail2384, label %bounds.ok2385

bounds.fail2384:                                  ; preds = %assign_bounds.ok2368
  %print_err2386 = call i32 (ptr, ...) @printf(ptr @err_msg.149)
  ret ptr null

bounds.ok2385:                                    ; preds = %assign_bounds.ok2368
  %elem_ptr2387 = getelementptr i8, ptr %data_ptr2376, i64 %resolved_index2380
  %loaded_val2388 = load i8, ptr %elem_ptr2387, align 1
  %elem_ptr2389 = getelementptr i8, ptr %data_ptr2365, i64 %__where_res_i_0_load2361
  store i8 %loaded_val2388, ptr %elem_ptr2389, align 1
  %x_load2390 = load i64, ptr @__where_res_i_0, align 8
  %inc_add2391 = add i64 %x_load2390, 1
  store i64 %inc_add2391, ptr @__where_res_i_0, align 8
  br label %ifcont1206
}

declare noalias ptr @malloc(i64)

!0 = distinct !{!0, !1}
!1 = !{!"llvm.loop.vectorize.enable", i1 true}
!2 = distinct !{!2, !1}
!3 = distinct !{!3, !1}
