; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

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

define ptr @main_4() {
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
  %arr_header37 = call ptr @malloc(i64 24)
  %arr_data_raw38 = call ptr @malloc(i64 %final_malloc_size)
  %len_ptr39 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 0
  %cap_ptr40 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 1
  %data_field_ptr41 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header37, i32 0, i32 2
  store i64 0, ptr %len_ptr39, align 8
  store i64 %rows_length, ptr %cap_ptr40, align 8
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
  %df_header = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %0 = getelementptr inbounds nuw %dataframe, ptr %df_header, i32 0, i32 0
  store ptr %arr_header, ptr %0, align 8
  %1 = getelementptr inbounds nuw %dataframe, ptr %df_header, i32 0, i32 1
  store ptr %arr_header37, ptr %1, align 8
  %2 = getelementptr inbounds nuw %dataframe, ptr %df_header, i32 0, i32 2
  store ptr %arr_header42, ptr %2, align 8
  store ptr %df_header, ptr @__map_result, align 8
  store i64 0, ptr @__map_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__map_i_load = load i64, ptr @__map_i, align 8
  %__map_src_load84 = load ptr, ptr @__map_src, align 8
  %rows_ptr_field85 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_src_load84, i32 0, i32 1
  %rows_ptr86 = load ptr, ptr %rows_ptr_field85, align 8
  %rows_array_ptr87 = bitcast ptr %rows_ptr86 to ptr
  %rows_length_ptr88 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr87, i32 0, i32 0
  %rows_length89 = load i64, ptr %rows_length_ptr88, align 8
  %icmp_tmp = icmp slt i64 %__map_i_load, %rows_length89
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__map_src_load90 = load ptr, ptr @__map_src, align 8
  %__map_i_load91 = load i64, ptr @__map_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__map_src_load90, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %elem_ptr92 = getelementptr ptr, ptr %data, i64 %__map_i_load91
  %record = load ptr, ptr %elem_ptr92, align 8
  store ptr %record, ptr @__current_row, align 8
  %__map_result_load = load ptr, ptr @__map_result, align 8
  %__current_row_load = load ptr, ptr @__current_row, align 8
  %record_buffer = call ptr @malloc(i64 16)
  %__current_row_load93 = load ptr, ptr @__current_row, align 8
  %ptr_latitude = getelementptr ptr, ptr %__current_row_load93, i64 1
  %load_latitude_ptr = load ptr, ptr %ptr_latitude, align 8
  %val_latitude = load double, ptr %load_latitude_ptr, align 8
  %fsubtmp = fsub double %val_latitude, 1.000000e+02
  %field_mem = call ptr @malloc(i64 8)
  store double %fsubtmp, ptr %field_mem, align 8
  %field_ptr = getelementptr ptr, ptr %record_buffer, i64 0
  store ptr %field_mem, ptr %field_ptr, align 8
  %field_mem94 = call ptr @malloc(i64 8)
  store double 1.000000e+02, ptr %field_mem94, align 8
  %field_ptr95 = getelementptr ptr, ptr %record_buffer, i64 1
  store ptr %field_mem94, ptr %field_ptr95, align 8
  %__current_row_load96 = load ptr, ptr @__current_row, align 8
  %record_buffer97 = call ptr @malloc(i64 16)
  %__current_row_load98 = load ptr, ptr @__current_row, align 8
  %ptr_latitude99 = getelementptr ptr, ptr %__current_row_load98, i64 1
  %load_latitude_ptr100 = load ptr, ptr %ptr_latitude99, align 8
  %val_latitude101 = load double, ptr %load_latitude_ptr100, align 8
  %fsubtmp102 = fsub double %val_latitude101, 1.000000e+02
  %field_mem103 = call ptr @malloc(i64 8)
  store double %fsubtmp102, ptr %field_mem103, align 8
  %field_ptr104 = getelementptr ptr, ptr %record_buffer97, i64 0
  store ptr %field_mem103, ptr %field_ptr104, align 8
  %field_mem105 = call ptr @malloc(i64 8)
  store double 1.000000e+02, ptr %field_mem105, align 8
  %field_ptr106 = getelementptr ptr, ptr %record_buffer97, i64 1
  store ptr %field_mem105, ptr %field_ptr106, align 8
  %3 = getelementptr ptr, ptr %__current_row_load96, i64 0
  %loaded_field = load ptr, ptr %3, align 8
  %4 = getelementptr ptr, ptr %record_buffer97, i64 0
  %loaded_field107 = load ptr, ptr %4, align 8
  %5 = getelementptr ptr, ptr %record_buffer97, i64 1
  %loaded_field108 = load ptr, ptr %5, align 8
  %6 = getelementptr ptr, ptr %__current_row_load96, i64 3
  %loaded_field109 = load ptr, ptr %6, align 8
  %7 = getelementptr ptr, ptr %__current_row_load96, i64 4
  %loaded_field110 = load ptr, ptr %7, align 8
  %8 = getelementptr ptr, ptr %__current_row_load96, i64 5
  %loaded_field111 = load ptr, ptr %8, align 8
  %9 = getelementptr ptr, ptr %__current_row_load96, i64 6
  %loaded_field112 = load ptr, ptr %9, align 8
  %10 = getelementptr ptr, ptr %__current_row_load96, i64 7
  %loaded_field113 = load ptr, ptr %10, align 8
  %11 = getelementptr ptr, ptr %__current_row_load96, i64 8
  %loaded_field114 = load ptr, ptr %11, align 8
  %12 = getelementptr ptr, ptr %__current_row_load96, i64 9
  %loaded_field115 = load ptr, ptr %12, align 8
  %13 = getelementptr ptr, ptr %__current_row_load96, i64 10
  %loaded_field116 = load ptr, ptr %13, align 8
  %14 = getelementptr ptr, ptr %__current_row_load96, i64 11
  %loaded_field117 = load ptr, ptr %14, align 8
  %15 = getelementptr ptr, ptr %__current_row_load96, i64 12
  %loaded_field118 = load ptr, ptr %15, align 8
  %16 = getelementptr ptr, ptr %__current_row_load96, i64 13
  %loaded_field119 = load ptr, ptr %16, align 8
  %17 = getelementptr ptr, ptr %__current_row_load96, i64 14
  %loaded_field120 = load ptr, ptr %17, align 8
  %18 = getelementptr ptr, ptr %__current_row_load96, i64 15
  %loaded_field121 = load ptr, ptr %18, align 8
  %19 = getelementptr ptr, ptr %__current_row_load96, i64 16
  %loaded_field122 = load ptr, ptr %19, align 8
  %20 = getelementptr ptr, ptr %__current_row_load96, i64 17
  %loaded_field123 = load ptr, ptr %20, align 8
  %21 = getelementptr ptr, ptr %__current_row_load96, i64 18
  %loaded_field124 = load ptr, ptr %21, align 8
  %22 = getelementptr ptr, ptr %__current_row_load96, i64 19
  %loaded_field125 = load ptr, ptr %22, align 8
  %23 = getelementptr ptr, ptr %__current_row_load96, i64 20
  %loaded_field126 = load ptr, ptr %23, align 8
  %24 = getelementptr ptr, ptr %__current_row_load96, i64 21
  %loaded_field127 = load ptr, ptr %24, align 8
  %25 = getelementptr ptr, ptr %__current_row_load96, i64 22
  %loaded_field128 = load ptr, ptr %25, align 8
  %26 = getelementptr ptr, ptr %__current_row_load96, i64 23
  %loaded_field129 = load ptr, ptr %26, align 8
  %27 = getelementptr ptr, ptr %__current_row_load96, i64 24
  %loaded_field130 = load ptr, ptr %27, align 8
  %28 = getelementptr ptr, ptr %__current_row_load96, i64 25
  %loaded_field131 = load ptr, ptr %28, align 8
  %29 = getelementptr ptr, ptr %__current_row_load96, i64 26
  %loaded_field132 = load ptr, ptr %29, align 8
  %30 = getelementptr ptr, ptr %__current_row_load96, i64 27
  %loaded_field133 = load ptr, ptr %30, align 8
  %31 = getelementptr ptr, ptr %__current_row_load96, i64 28
  %loaded_field134 = load ptr, ptr %31, align 8
  %32 = getelementptr ptr, ptr %__current_row_load96, i64 29
  %loaded_field135 = load ptr, ptr %32, align 8
  %33 = getelementptr ptr, ptr %__current_row_load96, i64 30
  %loaded_field136 = load ptr, ptr %33, align 8
  %34 = getelementptr ptr, ptr %__current_row_load96, i64 31
  %loaded_field137 = load ptr, ptr %34, align 8
  %35 = getelementptr ptr, ptr %__current_row_load96, i64 32
  %loaded_field138 = load ptr, ptr %35, align 8
  %36 = getelementptr ptr, ptr %__current_row_load96, i64 33
  %loaded_field139 = load ptr, ptr %36, align 8
  %37 = getelementptr ptr, ptr %__current_row_load96, i64 34
  %loaded_field140 = load ptr, ptr %37, align 8
  %38 = getelementptr ptr, ptr %__current_row_load96, i64 35
  %loaded_field141 = load ptr, ptr %38, align 8
  %39 = getelementptr ptr, ptr %__current_row_load96, i64 36
  %loaded_field142 = load ptr, ptr %39, align 8
  %rec_ptr = call ptr @malloc(i64 296)
  %slot_0 = getelementptr ptr, ptr %rec_ptr, i64 0
  store ptr %loaded_field, ptr %slot_0, align 8
  %slot_1 = getelementptr ptr, ptr %rec_ptr, i64 1
  store ptr %loaded_field107, ptr %slot_1, align 8
  %slot_2 = getelementptr ptr, ptr %rec_ptr, i64 2
  store ptr %loaded_field108, ptr %slot_2, align 8
  %slot_3 = getelementptr ptr, ptr %rec_ptr, i64 3
  store ptr %loaded_field109, ptr %slot_3, align 8
  %slot_4 = getelementptr ptr, ptr %rec_ptr, i64 4
  store ptr %loaded_field110, ptr %slot_4, align 8
  %slot_5 = getelementptr ptr, ptr %rec_ptr, i64 5
  store ptr %loaded_field111, ptr %slot_5, align 8
  %slot_6 = getelementptr ptr, ptr %rec_ptr, i64 6
  store ptr %loaded_field112, ptr %slot_6, align 8
  %slot_7 = getelementptr ptr, ptr %rec_ptr, i64 7
  store ptr %loaded_field113, ptr %slot_7, align 8
  %slot_8 = getelementptr ptr, ptr %rec_ptr, i64 8
  store ptr %loaded_field114, ptr %slot_8, align 8
  %slot_9 = getelementptr ptr, ptr %rec_ptr, i64 9
  store ptr %loaded_field115, ptr %slot_9, align 8
  %slot_10 = getelementptr ptr, ptr %rec_ptr, i64 10
  store ptr %loaded_field116, ptr %slot_10, align 8
  %slot_11 = getelementptr ptr, ptr %rec_ptr, i64 11
  store ptr %loaded_field117, ptr %slot_11, align 8
  %slot_12 = getelementptr ptr, ptr %rec_ptr, i64 12
  store ptr %loaded_field118, ptr %slot_12, align 8
  %slot_13 = getelementptr ptr, ptr %rec_ptr, i64 13
  store ptr %loaded_field119, ptr %slot_13, align 8
  %slot_14 = getelementptr ptr, ptr %rec_ptr, i64 14
  store ptr %loaded_field120, ptr %slot_14, align 8
  %slot_15 = getelementptr ptr, ptr %rec_ptr, i64 15
  store ptr %loaded_field121, ptr %slot_15, align 8
  %slot_16 = getelementptr ptr, ptr %rec_ptr, i64 16
  store ptr %loaded_field122, ptr %slot_16, align 8
  %slot_17 = getelementptr ptr, ptr %rec_ptr, i64 17
  store ptr %loaded_field123, ptr %slot_17, align 8
  %slot_18 = getelementptr ptr, ptr %rec_ptr, i64 18
  store ptr %loaded_field124, ptr %slot_18, align 8
  %slot_19 = getelementptr ptr, ptr %rec_ptr, i64 19
  store ptr %loaded_field125, ptr %slot_19, align 8
  %slot_20 = getelementptr ptr, ptr %rec_ptr, i64 20
  store ptr %loaded_field126, ptr %slot_20, align 8
  %slot_21 = getelementptr ptr, ptr %rec_ptr, i64 21
  store ptr %loaded_field127, ptr %slot_21, align 8
  %slot_22 = getelementptr ptr, ptr %rec_ptr, i64 22
  store ptr %loaded_field128, ptr %slot_22, align 8
  %slot_23 = getelementptr ptr, ptr %rec_ptr, i64 23
  store ptr %loaded_field129, ptr %slot_23, align 8
  %slot_24 = getelementptr ptr, ptr %rec_ptr, i64 24
  store ptr %loaded_field130, ptr %slot_24, align 8
  %slot_25 = getelementptr ptr, ptr %rec_ptr, i64 25
  store ptr %loaded_field131, ptr %slot_25, align 8
  %slot_26 = getelementptr ptr, ptr %rec_ptr, i64 26
  store ptr %loaded_field132, ptr %slot_26, align 8
  %slot_27 = getelementptr ptr, ptr %rec_ptr, i64 27
  store ptr %loaded_field133, ptr %slot_27, align 8
  %slot_28 = getelementptr ptr, ptr %rec_ptr, i64 28
  store ptr %loaded_field134, ptr %slot_28, align 8
  %slot_29 = getelementptr ptr, ptr %rec_ptr, i64 29
  store ptr %loaded_field135, ptr %slot_29, align 8
  %slot_30 = getelementptr ptr, ptr %rec_ptr, i64 30
  store ptr %loaded_field136, ptr %slot_30, align 8
  %slot_31 = getelementptr ptr, ptr %rec_ptr, i64 31
  store ptr %loaded_field137, ptr %slot_31, align 8
  %slot_32 = getelementptr ptr, ptr %rec_ptr, i64 32
  store ptr %loaded_field138, ptr %slot_32, align 8
  %slot_33 = getelementptr ptr, ptr %rec_ptr, i64 33
  store ptr %loaded_field139, ptr %slot_33, align 8
  %slot_34 = getelementptr ptr, ptr %rec_ptr, i64 34
  store ptr %loaded_field140, ptr %slot_34, align 8
  %slot_35 = getelementptr ptr, ptr %rec_ptr, i64 35
  store ptr %loaded_field141, ptr %slot_35, align 8
  %slot_36 = getelementptr ptr, ptr %rec_ptr, i64 36
  store ptr %loaded_field142, ptr %slot_36, align 8
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__map_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr143 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr144 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr145 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len = load i64, ptr %len_ptr143, align 8
  %cap = load i64, ptr %cap_ptr144, align 8
  %data146 = load ptr, ptr %data_ptr_ptr145, align 8
  %is_full = icmp uge i64 %len, %cap
  br i1 %is_full, label %grow, label %cont

for.step:                                         ; preds = %cont
  %x_load = load i64, ptr @__map_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__map_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__map_result_load147 = load ptr, ptr @__map_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__map_result_load147, ptr %data_ptr, align 8
  ret ptr %runtime_obj

grow:                                             ; preds = %for.body
  %40 = icmp eq i64 %cap, 0
  %41 = mul i64 %cap, 2
  %new_cap = select i1 %40, i64 4, i64 %41
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data146, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr144, align 8
  store ptr %realloc, ptr %data_ptr_ptr145, align 8
  br label %cont

cont:                                             ; preds = %grow, %for.body
  %final_data_ptr = phi ptr [ %data146, %for.body ], [ %realloc, %grow ]
  %target = getelementptr ptr, ptr %final_data_ptr, i64 %len
  store ptr %rec_ptr, ptr %target, align 8
  %new_len = add i64 %len, 1
  store i64 %new_len, ptr %len_ptr143, align 8
  br label %for.step
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i1 true}
