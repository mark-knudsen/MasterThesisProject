; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@df = external global ptr
@__where_src = external global ptr, align 8
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
@__where_result = external global ptr, align 8
@__where_i = external global i64, align 8
@df_idx_err_msg = private unnamed_addr constant [50 x i8] c"Runtime Error: Dataframe row index out of bounds\0A\00", align 1
@df_idx_err_msg.37 = private unnamed_addr constant [50 x i8] c"Runtime Error: Dataframe row index out of bounds\0A\00", align 1
@df_idx_err_msg.38 = private unnamed_addr constant [50 x i8] c"Runtime Error: Dataframe row index out of bounds\0A\00", align 1

define ptr @main_7() {
entry:
  %df_load = load ptr, ptr @df, align 8
  store ptr %df_load, ptr @__where_src, align 8
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
  %__where_src_load = load ptr, ptr @__where_src, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_src_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  %__where_src_load37 = load ptr, ptr @__where_src, align 8
  %rows_ptr_field38 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_src_load37, i32 0, i32 1
  %rows_ptr39 = load ptr, ptr %rows_ptr_field38, align 8
  %rows_array_ptr40 = bitcast ptr %rows_ptr39 to ptr
  %rows_length_ptr41 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr40, i32 0, i32 0
  %rows_length42 = load i64, ptr %rows_length_ptr41, align 8
  %total_size = mul i64 8, %rows_length42
  %padded_size = add i64 %total_size, 31
  %aligned_size = and i64 %padded_size, -32
  %is_zero = icmp eq i64 %aligned_size, 0
  %final_malloc_size = select i1 %is_zero, i64 8, i64 %aligned_size
  %arr_header43 = call ptr @malloc(i64 24)
  %arr_data_raw44 = call ptr @malloc(i64 %final_malloc_size)
  %len_ptr45 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header43, i32 0, i32 0
  %cap_ptr46 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header43, i32 0, i32 1
  %data_field_ptr47 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header43, i32 0, i32 2
  store i64 0, ptr %len_ptr45, align 8
  store i64 %rows_length42, ptr %cap_ptr46, align 8
  store ptr %arr_data_raw44, ptr %data_field_ptr47, align 8
  %arr_header48 = call ptr @malloc(i64 24)
  %arr_data_raw49 = call ptr @malloc(i64 320)
  %len_ptr50 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header48, i32 0, i32 0
  %cap_ptr51 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header48, i32 0, i32 1
  %data_field_ptr52 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header48, i32 0, i32 2
  store i64 37, ptr %len_ptr50, align 8
  store i64 37, ptr %cap_ptr51, align 8
  store ptr %arr_data_raw49, ptr %data_field_ptr52, align 8
  %elem_ptr53 = getelementptr i64, ptr %arr_data_raw49, i64 0
  store i64 4, ptr %elem_ptr53, align 8
  %elem_ptr54 = getelementptr i64, ptr %arr_data_raw49, i64 1
  store i64 2, ptr %elem_ptr54, align 8
  %elem_ptr55 = getelementptr i64, ptr %arr_data_raw49, i64 2
  store i64 2, ptr %elem_ptr55, align 8
  %elem_ptr56 = getelementptr i64, ptr %arr_data_raw49, i64 3
  store i64 2, ptr %elem_ptr56, align 8
  %elem_ptr57 = getelementptr i64, ptr %arr_data_raw49, i64 4
  store i64 2, ptr %elem_ptr57, align 8
  %elem_ptr58 = getelementptr i64, ptr %arr_data_raw49, i64 5
  store i64 2, ptr %elem_ptr58, align 8
  %elem_ptr59 = getelementptr i64, ptr %arr_data_raw49, i64 6
  store i64 2, ptr %elem_ptr59, align 8
  %elem_ptr60 = getelementptr i64, ptr %arr_data_raw49, i64 7
  store i64 2, ptr %elem_ptr60, align 8
  %elem_ptr61 = getelementptr i64, ptr %arr_data_raw49, i64 8
  store i64 2, ptr %elem_ptr61, align 8
  %elem_ptr62 = getelementptr i64, ptr %arr_data_raw49, i64 9
  store i64 2, ptr %elem_ptr62, align 8
  %elem_ptr63 = getelementptr i64, ptr %arr_data_raw49, i64 10
  store i64 2, ptr %elem_ptr63, align 8
  %elem_ptr64 = getelementptr i64, ptr %arr_data_raw49, i64 11
  store i64 2, ptr %elem_ptr64, align 8
  %elem_ptr65 = getelementptr i64, ptr %arr_data_raw49, i64 12
  store i64 2, ptr %elem_ptr65, align 8
  %elem_ptr66 = getelementptr i64, ptr %arr_data_raw49, i64 13
  store i64 2, ptr %elem_ptr66, align 8
  %elem_ptr67 = getelementptr i64, ptr %arr_data_raw49, i64 14
  store i64 2, ptr %elem_ptr67, align 8
  %elem_ptr68 = getelementptr i64, ptr %arr_data_raw49, i64 15
  store i64 2, ptr %elem_ptr68, align 8
  %elem_ptr69 = getelementptr i64, ptr %arr_data_raw49, i64 16
  store i64 2, ptr %elem_ptr69, align 8
  %elem_ptr70 = getelementptr i64, ptr %arr_data_raw49, i64 17
  store i64 2, ptr %elem_ptr70, align 8
  %elem_ptr71 = getelementptr i64, ptr %arr_data_raw49, i64 18
  store i64 2, ptr %elem_ptr71, align 8
  %elem_ptr72 = getelementptr i64, ptr %arr_data_raw49, i64 19
  store i64 2, ptr %elem_ptr72, align 8
  %elem_ptr73 = getelementptr i64, ptr %arr_data_raw49, i64 20
  store i64 1, ptr %elem_ptr73, align 8
  %elem_ptr74 = getelementptr i64, ptr %arr_data_raw49, i64 21
  store i64 3, ptr %elem_ptr74, align 8
  %elem_ptr75 = getelementptr i64, ptr %arr_data_raw49, i64 22
  store i64 3, ptr %elem_ptr75, align 8
  %elem_ptr76 = getelementptr i64, ptr %arr_data_raw49, i64 23
  store i64 3, ptr %elem_ptr76, align 8
  %elem_ptr77 = getelementptr i64, ptr %arr_data_raw49, i64 24
  store i64 3, ptr %elem_ptr77, align 8
  %elem_ptr78 = getelementptr i64, ptr %arr_data_raw49, i64 25
  store i64 3, ptr %elem_ptr78, align 8
  %elem_ptr79 = getelementptr i64, ptr %arr_data_raw49, i64 26
  store i64 3, ptr %elem_ptr79, align 8
  %elem_ptr80 = getelementptr i64, ptr %arr_data_raw49, i64 27
  store i64 3, ptr %elem_ptr80, align 8
  %elem_ptr81 = getelementptr i64, ptr %arr_data_raw49, i64 28
  store i64 3, ptr %elem_ptr81, align 8
  %elem_ptr82 = getelementptr i64, ptr %arr_data_raw49, i64 29
  store i64 3, ptr %elem_ptr82, align 8
  %elem_ptr83 = getelementptr i64, ptr %arr_data_raw49, i64 30
  store i64 3, ptr %elem_ptr83, align 8
  %elem_ptr84 = getelementptr i64, ptr %arr_data_raw49, i64 31
  store i64 3, ptr %elem_ptr84, align 8
  %elem_ptr85 = getelementptr i64, ptr %arr_data_raw49, i64 32
  store i64 3, ptr %elem_ptr85, align 8
  %elem_ptr86 = getelementptr i64, ptr %arr_data_raw49, i64 33
  store i64 3, ptr %elem_ptr86, align 8
  %elem_ptr87 = getelementptr i64, ptr %arr_data_raw49, i64 34
  store i64 3, ptr %elem_ptr87, align 8
  %elem_ptr88 = getelementptr i64, ptr %arr_data_raw49, i64 35
  store i64 3, ptr %elem_ptr88, align 8
  %elem_ptr89 = getelementptr i64, ptr %arr_data_raw49, i64 36
  store i64 3, ptr %elem_ptr89, align 8
  %df_header = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %0 = getelementptr inbounds nuw %dataframe, ptr %df_header, i32 0, i32 0
  store ptr %arr_header, ptr %0, align 8
  %1 = getelementptr inbounds nuw %dataframe, ptr %df_header, i32 0, i32 1
  store ptr %arr_header43, ptr %1, align 8
  %2 = getelementptr inbounds nuw %dataframe, ptr %df_header, i32 0, i32 2
  store ptr %arr_header48, ptr %2, align 8
  store ptr %df_header, ptr @__where_result, align 8
  store i64 0, ptr @__where_i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__where_i_load = load i64, ptr @__where_i, align 8
  %__where_src_load90 = load ptr, ptr @__where_src, align 8
  %rows_ptr_field91 = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_src_load90, i32 0, i32 1
  %rows_ptr92 = load ptr, ptr %rows_ptr_field91, align 8
  %rows_array_ptr93 = bitcast ptr %rows_ptr92 to ptr
  %rows_length_ptr94 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr93, i32 0, i32 0
  %rows_length95 = load i64, ptr %rows_length_ptr94, align 8
  %icmp_tmp = icmp slt i64 %__where_i_load, %rows_length95
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__where_src_load96 = load ptr, ptr @__where_src, align 8
  %__where_i_load97 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %__where_src_load96, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %len_ptr98 = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr98, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data = load ptr, ptr %data_ptr_ptr, align 8
  %in_bounds = icmp ult i64 %__where_i_load97, %len
  br i1 %in_bounds, label %df_idx_ok, label %df_idx_err

for.step:                                         ; preds = %ifcont
  %x_load = load i64, ptr @__where_i, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__where_i, align 8
  br label %for.cond, !llvm.loop !0

for.end:                                          ; preds = %for.cond
  %__where_result_load138 = load ptr, ptr @__where_result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__where_result_load138, ptr %data_ptr, align 8
  ret ptr %runtime_obj

df_idx_ok:                                        ; preds = %for.body
  %elem_ptr99 = getelementptr ptr, ptr %data, i64 %__where_i_load97
  %record = load ptr, ptr %elem_ptr99, align 8
  br label %df_idx_merge

df_idx_err:                                       ; preds = %for.body
  %print_err = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg)
  br label %df_idx_merge

df_idx_merge:                                     ; preds = %df_idx_ok, %df_idx_err
  %df_idx_result = phi ptr [ null, %df_idx_err ], [ %record, %df_idx_ok ]
  %ptr_latitude = getelementptr ptr, ptr %df_idx_result, i64 1
  %load_latitude_ptr = load ptr, ptr %ptr_latitude, align 8
  %val_latitude = load double, ptr %load_latitude_ptr, align 8
  %fcmp_tmp = fcmp ogt double %val_latitude, -1.800000e+01
  %__where_src_load100 = load ptr, ptr @__where_src, align 8
  %__where_i_load101 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr102 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load100, i32 0, i32 1
  %rows103 = load ptr, ptr %rows_ptr_ptr102, align 8
  %len_ptr104 = getelementptr inbounds nuw %array, ptr %rows103, i32 0, i32 0
  %len105 = load i64, ptr %len_ptr104, align 8
  %data_ptr_ptr106 = getelementptr inbounds nuw %array, ptr %rows103, i32 0, i32 2
  %data107 = load ptr, ptr %data_ptr_ptr106, align 8
  %in_bounds108 = icmp ult i64 %__where_i_load101, %len105
  br i1 %in_bounds108, label %df_idx_ok109, label %df_idx_err110

df_idx_ok109:                                     ; preds = %df_idx_merge
  %elem_ptr113 = getelementptr ptr, ptr %data107, i64 %__where_i_load101
  %record114 = load ptr, ptr %elem_ptr113, align 8
  br label %df_idx_merge111

df_idx_err110:                                    ; preds = %df_idx_merge
  %print_err112 = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg.37)
  br label %df_idx_merge111

df_idx_merge111:                                  ; preds = %df_idx_ok109, %df_idx_err110
  %df_idx_result115 = phi ptr [ null, %df_idx_err110 ], [ %record114, %df_idx_ok109 ]
  %ptr_longitude = getelementptr ptr, ptr %df_idx_result115, i64 2
  %load_longitude_ptr = load ptr, ptr %ptr_longitude, align 8
  %val_longitude = load double, ptr %load_longitude_ptr, align 8
  %fcmp_tmp116 = fcmp olt double %val_longitude, -6.900000e+01
  %andtmp = and i1 %fcmp_tmp, %fcmp_tmp116
  br i1 %andtmp, label %then, label %else

then:                                             ; preds = %df_idx_merge111
  %__where_result_load = load ptr, ptr @__where_result, align 8
  %__where_src_load117 = load ptr, ptr @__where_src, align 8
  %__where_i_load118 = load i64, ptr @__where_i, align 8
  %rows_ptr_ptr119 = getelementptr inbounds nuw %dataframe, ptr %__where_src_load117, i32 0, i32 1
  %rows120 = load ptr, ptr %rows_ptr_ptr119, align 8
  %len_ptr121 = getelementptr inbounds nuw %array, ptr %rows120, i32 0, i32 0
  %len122 = load i64, ptr %len_ptr121, align 8
  %data_ptr_ptr123 = getelementptr inbounds nuw %array, ptr %rows120, i32 0, i32 2
  %data124 = load ptr, ptr %data_ptr_ptr123, align 8
  %in_bounds125 = icmp ult i64 %__where_i_load118, %len122
  br i1 %in_bounds125, label %df_idx_ok126, label %df_idx_err127

else:                                             ; preds = %df_idx_merge111
  br label %ifcont

ifcont:                                           ; preds = %else, %cont
  %iftmp = phi ptr [ %__where_result_load, %cont ], [ 0.000000e+00, %else ]
  br label %for.step

df_idx_ok126:                                     ; preds = %then
  %elem_ptr130 = getelementptr ptr, ptr %data124, i64 %__where_i_load118
  %record131 = load ptr, ptr %elem_ptr130, align 8
  br label %df_idx_merge128

df_idx_err127:                                    ; preds = %then
  %print_err129 = call i32 (ptr, ...) @printf(ptr @df_idx_err_msg.38)
  br label %df_idx_merge128

df_idx_merge128:                                  ; preds = %df_idx_ok126, %df_idx_err127
  %df_idx_result132 = phi ptr [ null, %df_idx_err127 ], [ %record131, %df_idx_ok126 ]
  %rows_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %__where_result_load, i32 0, i32 1
  %rows_array = load ptr, ptr %rows_field, align 8
  %len_ptr133 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 0
  %cap_ptr134 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 1
  %data_ptr_ptr135 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array, i32 0, i32 2
  %len136 = load i64, ptr %len_ptr133, align 8
  %cap = load i64, ptr %cap_ptr134, align 8
  %data137 = load ptr, ptr %data_ptr_ptr135, align 8
  %is_full = icmp uge i64 %len136, %cap
  br i1 %is_full, label %grow, label %cont

grow:                                             ; preds = %df_idx_merge128
  %3 = icmp eq i64 %cap, 0
  %4 = mul i64 %cap, 2
  %new_cap = select i1 %3, i64 4, i64 %4
  %bytes = mul i64 %new_cap, 8
  %realloc = call ptr @realloc(ptr %data137, i64 %bytes)
  store i64 %new_cap, ptr %cap_ptr134, align 8
  store ptr %realloc, ptr %data_ptr_ptr135, align 8
  br label %cont

cont:                                             ; preds = %grow, %df_idx_merge128
  %final_data_ptr = phi ptr [ %data137, %df_idx_merge128 ], [ %realloc, %grow ]
  %target = getelementptr ptr, ptr %final_data_ptr, i64 %len136
  store ptr %df_idx_result132, ptr %target, align 8
  %new_len = add i64 %len136, 1
  store i64 %new_len, ptr %len_ptr133, align 8
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)

!0 = !{!"loop.id", !1}
!1 = !{!"llvm.loop.vectorize.enable", i1 true}
