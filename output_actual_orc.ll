; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr, i64 }
%struct_name_age_hasJob_savings = type { ptr, i64, i1, i64 }

@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.2 = private unnamed_addr constant [7 x i8] c"hasJob\00", align 1
@str.3 = private unnamed_addr constant [8 x i8] c"savings\00", align 1
@__result_7c35 = global ptr null
@__i_7c35 = global i64 0
@str.4 = private unnamed_addr constant [20 x i8] c"main/CSV/mytest.csv\00", align 1
@csv_schema = private unnamed_addr constant [5 x i8] c"SIBI\00", align 1
@str.5 = private unnamed_addr constant [20 x i8] c"main/CSV/mytest.csv\00", align 1
@csv_schema.6 = private unnamed_addr constant [5 x i8] c"SIBI\00", align 1
@str.7 = private unnamed_addr constant [12 x i8] c"Hary potter\00", align 1
@str.8 = private unnamed_addr constant [20 x i8] c"main/CSV/mytest.csv\00", align 1
@csv_schema.9 = private unnamed_addr constant [5 x i8] c"SIBI\00", align 1

define ptr @main_0() {
entry:
  %arr_header = call ptr @malloc(i64 24)
  %arr_data = call ptr @malloc(i64 800)
  %0 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 0
  %1 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 1
  %2 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header, i32 0, i32 2
  store i64 0, ptr %0, align 4
  store i64 100, ptr %1, align 4
  store ptr %arr_data, ptr %2, align 8
  %arr_header1 = call ptr @malloc(i64 24)
  %arr_data2 = call ptr @malloc(i64 800)
  %3 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 0
  %4 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 1
  %5 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header1, i32 0, i32 2
  store i64 0, ptr %3, align 4
  store i64 100, ptr %4, align 4
  store ptr %arr_data2, ptr %5, align 8
  %arr_header3 = call ptr @malloc(i64 24)
  %arr_data4 = call ptr @malloc(i64 800)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 0
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 1
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header3, i32 0, i32 2
  store i64 0, ptr %6, align 4
  store i64 100, ptr %7, align 4
  store ptr %arr_data4, ptr %8, align 8
  %arr_header5 = call ptr @malloc(i64 24)
  %arr_data6 = call ptr @malloc(i64 800)
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 0
  %10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 1
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header5, i32 0, i32 2
  store i64 0, ptr %9, align 4
  store i64 100, ptr %10, align 4
  store ptr %arr_data6, ptr %11, align 8
  %data_header = call ptr @malloc(i64 24)
  %data_buffer = call ptr @malloc(i64 32)
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 0
  store i64 4, ptr %12, align 4
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 1
  store i64 4, ptr %13, align 4
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_header, i32 0, i32 2
  store ptr %data_buffer, ptr %14, align 8
  %data_gep = getelementptr ptr, ptr %data_buffer, i64 0
  store ptr %arr_header, ptr %data_gep, align 8
  %data_gep7 = getelementptr ptr, ptr %data_buffer, i64 1
  store ptr %arr_header1, ptr %data_gep7, align 8
  %data_gep8 = getelementptr ptr, ptr %data_buffer, i64 2
  store ptr %arr_header3, ptr %data_gep8, align 8
  %data_gep9 = getelementptr ptr, ptr %data_buffer, i64 3
  store ptr %arr_header5, ptr %data_gep9, align 8
  %arr_header10 = call ptr @malloc(i64 24)
  %arr_data11 = call ptr @malloc(i64 32)
  %15 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header10, i32 0, i32 0
  %16 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header10, i32 0, i32 1
  %17 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header10, i32 0, i32 2
  store i64 4, ptr %15, align 4
  store i64 4, ptr %16, align 4
  store ptr %arr_data11, ptr %17, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data11, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr12 = getelementptr ptr, ptr %arr_data11, i64 1
  store ptr @str.1, ptr %elem_ptr12, align 8
  %elem_ptr13 = getelementptr ptr, ptr %arr_data11, i64 2
  store ptr @str.2, ptr %elem_ptr13, align 8
  %elem_ptr14 = getelementptr ptr, ptr %arr_data11, i64 3
  store ptr @str.3, ptr %elem_ptr14, align 8
  %arr_header15 = call ptr @malloc(i64 24)
  %arr_data16 = call ptr @malloc(i64 32)
  %18 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 0
  %19 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 1
  %20 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header15, i32 0, i32 2
  store i64 4, ptr %18, align 4
  store i64 4, ptr %19, align 4
  store ptr %arr_data16, ptr %20, align 8
  %elem_ptr17 = getelementptr ptr, ptr %arr_data16, i64 0
  store ptr null, ptr %elem_ptr17, align 8
  %elem_ptr18 = getelementptr ptr, ptr %arr_data16, i64 1
  store ptr null, ptr %elem_ptr18, align 8
  %elem_ptr19 = getelementptr ptr, ptr %arr_data16, i64 2
  store ptr null, ptr %elem_ptr19, align 8
  %elem_ptr20 = getelementptr ptr, ptr %arr_data16, i64 3
  store ptr null, ptr %elem_ptr20, align 8
  %df_instance = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %21 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 0
  store ptr %arr_header10, ptr %21, align 8
  %22 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 1
  store ptr %data_header, ptr %22, align 8
  %23 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 2
  store ptr %arr_header15, ptr %23, align 8
  %24 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 3
  store i64 0, ptr %24, align 4
  store ptr %df_instance, ptr @__result_7c35, align 8
  store i64 0, ptr @__i_7c35, align 8
  store i64 0, ptr @__i_7c35, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__i_7c35_load = load i64, ptr @__i_7c35, align 4
  %csv_boxed = call ptr @ReadCsvInternal(ptr @str.4, ptr @csv_schema)
  %25 = getelementptr inbounds nuw { i64, ptr }, ptr %csv_boxed, i32 0, i32 1
  %df_ptr = load ptr, ptr %25, align 8
  %rowCount_ptr = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %df_ptr, i32 0, i32 3
  %rowCount = load i64, ptr %rowCount_ptr, align 8
  %icmp_tmp = icmp slt i64 %__i_7c35_load, %rowCount
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__i_7c35_load21 = load i64, ptr @__i_7c35, align 4
  %csv_boxed22 = call ptr @ReadCsvInternal(ptr @str.5, ptr @csv_schema.6)
  %26 = getelementptr inbounds nuw { i64, ptr }, ptr %csv_boxed22, i32 0, i32 1
  %df_ptr23 = load ptr, ptr %26, align 8
  %data_ptrs_ptr = getelementptr inbounds nuw %dataframe, ptr %df_ptr23, i32 0, i32 1
  %header_ptr = load ptr, ptr %data_ptrs_ptr, align 8
  %data_ptrs_data_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %header_ptr, i32 0, i32 2
  %data_ptrs_raw = load ptr, ptr %data_ptrs_data_ptr, align 8
  %row = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age_hasJob_savings, ptr null, i32 1) to i32))
  %col_ptr_ptr = getelementptr ptr, ptr %data_ptrs_raw, i64 0
  %col_array_header = load ptr, ptr %col_ptr_ptr, align 8
  %col_data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header, i32 0, i32 2
  %col_data_raw = load ptr, ptr %col_data_ptr_ptr, align 8
  %elem_ptr24 = getelementptr ptr, ptr %col_data_raw, i64 %__i_7c35_load21
  %val = load ptr, ptr %elem_ptr24, align 8
  %field_ptr = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row, i32 0, i32 0
  store ptr %val, ptr %field_ptr, align 8
  %col_ptr_ptr25 = getelementptr ptr, ptr %data_ptrs_raw, i64 1
  %col_array_header26 = load ptr, ptr %col_ptr_ptr25, align 8
  %col_data_ptr_ptr27 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header26, i32 0, i32 2
  %col_data_raw28 = load ptr, ptr %col_data_ptr_ptr27, align 8
  %elem_ptr29 = getelementptr i64, ptr %col_data_raw28, i64 %__i_7c35_load21
  %val30 = load i64, ptr %elem_ptr29, align 4
  %field_ptr31 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row, i32 0, i32 1
  store i64 %val30, ptr %field_ptr31, align 4
  %col_ptr_ptr32 = getelementptr ptr, ptr %data_ptrs_raw, i64 2
  %col_array_header33 = load ptr, ptr %col_ptr_ptr32, align 8
  %col_data_ptr_ptr34 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header33, i32 0, i32 2
  %col_data_raw35 = load ptr, ptr %col_data_ptr_ptr34, align 8
  %elem_ptr36 = getelementptr i8, ptr %col_data_raw35, i64 %__i_7c35_load21
  %raw = load i8, ptr %elem_ptr36, align 1
  %bool = trunc i8 %raw to i1
  %field_ptr37 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row, i32 0, i32 2
  store i1 %bool, ptr %field_ptr37, align 1
  %col_ptr_ptr38 = getelementptr ptr, ptr %data_ptrs_raw, i64 3
  %col_array_header39 = load ptr, ptr %col_ptr_ptr38, align 8
  %col_data_ptr_ptr40 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header39, i32 0, i32 2
  %col_data_raw41 = load ptr, ptr %col_data_ptr_ptr40, align 8
  %elem_ptr42 = getelementptr i64, ptr %col_data_raw41, i64 %__i_7c35_load21
  %val43 = load i64, ptr %elem_ptr42, align 4
  %field_ptr44 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row, i32 0, i32 3
  store i64 %val43, ptr %field_ptr44, align 4
  %ptr_name = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row, i32 0, i32 0
  %val_name = load ptr, ptr %ptr_name, align 8
  %strcmp_res = call i32 @strcmp(ptr %val_name, ptr @str.7)
  %str_eq = icmp eq i32 %strcmp_res, 0
  br i1 %str_eq, label %then, label %else

for.step:                                         ; preds = %ifcont
  %x_load = load i64, ptr @__i_7c35, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__i_7c35, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__result_7c35_load134 = load ptr, ptr @__result_7c35, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__result_7c35_load134, ptr %data_ptr, align 8
  ret ptr %runtime_obj

then:                                             ; preds = %for.body
  %__i_7c35_load45 = load i64, ptr @__i_7c35, align 4
  %csv_boxed46 = call ptr @ReadCsvInternal(ptr @str.8, ptr @csv_schema.9)
  %27 = getelementptr inbounds nuw { i64, ptr }, ptr %csv_boxed46, i32 0, i32 1
  %df_ptr47 = load ptr, ptr %27, align 8
  %data_ptrs_ptr48 = getelementptr inbounds nuw %dataframe, ptr %df_ptr47, i32 0, i32 1
  %header_ptr49 = load ptr, ptr %data_ptrs_ptr48, align 8
  %data_ptrs_data_ptr50 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %header_ptr49, i32 0, i32 2
  %data_ptrs_raw51 = load ptr, ptr %data_ptrs_data_ptr50, align 8
  %row52 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age_hasJob_savings, ptr null, i32 1) to i32))
  %col_ptr_ptr53 = getelementptr ptr, ptr %data_ptrs_raw51, i64 0
  %col_array_header54 = load ptr, ptr %col_ptr_ptr53, align 8
  %col_data_ptr_ptr55 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header54, i32 0, i32 2
  %col_data_raw56 = load ptr, ptr %col_data_ptr_ptr55, align 8
  %elem_ptr57 = getelementptr ptr, ptr %col_data_raw56, i64 %__i_7c35_load45
  %val58 = load ptr, ptr %elem_ptr57, align 8
  %field_ptr59 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row52, i32 0, i32 0
  store ptr %val58, ptr %field_ptr59, align 8
  %col_ptr_ptr60 = getelementptr ptr, ptr %data_ptrs_raw51, i64 1
  %col_array_header61 = load ptr, ptr %col_ptr_ptr60, align 8
  %col_data_ptr_ptr62 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header61, i32 0, i32 2
  %col_data_raw63 = load ptr, ptr %col_data_ptr_ptr62, align 8
  %elem_ptr64 = getelementptr i64, ptr %col_data_raw63, i64 %__i_7c35_load45
  %val65 = load i64, ptr %elem_ptr64, align 4
  %field_ptr66 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row52, i32 0, i32 1
  store i64 %val65, ptr %field_ptr66, align 4
  %col_ptr_ptr67 = getelementptr ptr, ptr %data_ptrs_raw51, i64 2
  %col_array_header68 = load ptr, ptr %col_ptr_ptr67, align 8
  %col_data_ptr_ptr69 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header68, i32 0, i32 2
  %col_data_raw70 = load ptr, ptr %col_data_ptr_ptr69, align 8
  %elem_ptr71 = getelementptr i8, ptr %col_data_raw70, i64 %__i_7c35_load45
  %raw72 = load i8, ptr %elem_ptr71, align 1
  %bool73 = trunc i8 %raw72 to i1
  %field_ptr74 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row52, i32 0, i32 2
  store i1 %bool73, ptr %field_ptr74, align 1
  %col_ptr_ptr75 = getelementptr ptr, ptr %data_ptrs_raw51, i64 3
  %col_array_header76 = load ptr, ptr %col_ptr_ptr75, align 8
  %col_data_ptr_ptr77 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header76, i32 0, i32 2
  %col_data_raw78 = load ptr, ptr %col_data_ptr_ptr77, align 8
  %elem_ptr79 = getelementptr i64, ptr %col_data_raw78, i64 %__i_7c35_load45
  %val80 = load i64, ptr %elem_ptr79, align 4
  %field_ptr81 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row52, i32 0, i32 3
  store i64 %val80, ptr %field_ptr81, align 4
  %__result_7c35_load = load ptr, ptr @__result_7c35, align 8
  %28 = getelementptr inbounds nuw %dataframe, ptr %__result_7c35_load, i32 0, i32 1
  %29 = load ptr, ptr %28, align 8
  %data_array = bitcast ptr %29 to ptr
  %30 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_array, i32 0, i32 2
  %31 = load ptr, ptr %30, align 8
  %32 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row52, i32 0, i32 0
  %33 = load ptr, ptr %32, align 8
  %col_ptr_ptr82 = getelementptr ptr, ptr %31, i64 0
  %34 = load ptr, ptr %col_ptr_ptr82, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %34, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %34, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %34, i32 0, i32 2
  %curr_len = load i64, ptr %len_ptr, align 4
  %curr_cap = load i64, ptr %cap_ptr, align 4
  %curr_data = load ptr, ptr %data_ptr_ptr, align 8
  %needs_grow = icmp sge i64 %curr_len, %curr_cap
  br i1 %needs_grow, label %grow, label %store_element

else:                                             ; preds = %for.body
  br label %ifcont

ifcont:                                           ; preds = %else, %store_element126
  %iftmp = phi ptr [ %__result_7c35_load, %store_element126 ], [ null, %else ]
  br label %for.step

grow:                                             ; preds = %then
  %35 = icmp eq i64 %curr_cap, 0
  %36 = mul i64 %curr_cap, 2
  %new_cap = select i1 %35, i64 4, i64 %36
  %new_byte_count = mul i64 %new_cap, 8
  %reallocated_data = call ptr @realloc(ptr %curr_data, i64 %new_byte_count)
  store i64 %new_cap, ptr %cap_ptr, align 4
  store ptr %reallocated_data, ptr %data_ptr_ptr, align 8
  br label %store_element

store_element:                                    ; preds = %grow, %then
  %final_data = load ptr, ptr %data_ptr_ptr, align 8
  %offset = mul i64 %curr_len, 8
  %raw_elem_ptr = getelementptr i8, ptr %final_data, i64 %offset
  store ptr %33, ptr %raw_elem_ptr, align 8
  %new_len = add i64 %curr_len, 1
  store i64 %new_len, ptr %len_ptr, align 4
  %37 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row52, i32 0, i32 1
  %38 = load i64, ptr %37, align 4
  %col_ptr_ptr83 = getelementptr ptr, ptr %31, i64 1
  %39 = load ptr, ptr %col_ptr_ptr83, align 8
  %len_ptr84 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %39, i32 0, i32 0
  %cap_ptr85 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %39, i32 0, i32 1
  %data_ptr_ptr86 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %39, i32 0, i32 2
  %curr_len87 = load i64, ptr %len_ptr84, align 4
  %curr_cap88 = load i64, ptr %cap_ptr85, align 4
  %curr_data89 = load ptr, ptr %data_ptr_ptr86, align 8
  %needs_grow90 = icmp sge i64 %curr_len87, %curr_cap88
  br i1 %needs_grow90, label %grow91, label %store_element92

grow91:                                           ; preds = %store_element
  %40 = icmp eq i64 %curr_cap88, 0
  %41 = mul i64 %curr_cap88, 2
  %new_cap93 = select i1 %40, i64 4, i64 %41
  %new_byte_count94 = mul i64 %new_cap93, 8
  %reallocated_data95 = call ptr @realloc(ptr %curr_data89, i64 %new_byte_count94)
  store i64 %new_cap93, ptr %cap_ptr85, align 4
  store ptr %reallocated_data95, ptr %data_ptr_ptr86, align 8
  br label %store_element92

store_element92:                                  ; preds = %grow91, %store_element
  %final_data96 = load ptr, ptr %data_ptr_ptr86, align 8
  %offset97 = mul i64 %curr_len87, 8
  %raw_elem_ptr98 = getelementptr i8, ptr %final_data96, i64 %offset97
  store i64 %38, ptr %raw_elem_ptr98, align 4
  %new_len99 = add i64 %curr_len87, 1
  store i64 %new_len99, ptr %len_ptr84, align 4
  %42 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row52, i32 0, i32 2
  %43 = load i1, ptr %42, align 1
  %col_ptr_ptr100 = getelementptr ptr, ptr %31, i64 2
  %44 = load ptr, ptr %col_ptr_ptr100, align 8
  %len_ptr101 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %44, i32 0, i32 0
  %cap_ptr102 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %44, i32 0, i32 1
  %data_ptr_ptr103 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %44, i32 0, i32 2
  %curr_len104 = load i64, ptr %len_ptr101, align 4
  %curr_cap105 = load i64, ptr %cap_ptr102, align 4
  %curr_data106 = load ptr, ptr %data_ptr_ptr103, align 8
  %needs_grow107 = icmp sge i64 %curr_len104, %curr_cap105
  br i1 %needs_grow107, label %grow108, label %store_element109

grow108:                                          ; preds = %store_element92
  %45 = icmp eq i64 %curr_cap105, 0
  %46 = mul i64 %curr_cap105, 2
  %new_cap110 = select i1 %45, i64 4, i64 %46
  %new_byte_count111 = mul i64 %new_cap110, 1
  %reallocated_data112 = call ptr @realloc(ptr %curr_data106, i64 %new_byte_count111)
  store i64 %new_cap110, ptr %cap_ptr102, align 4
  store ptr %reallocated_data112, ptr %data_ptr_ptr103, align 8
  br label %store_element109

store_element109:                                 ; preds = %grow108, %store_element92
  %final_data113 = load ptr, ptr %data_ptr_ptr103, align 8
  %offset114 = mul i64 %curr_len104, 1
  %raw_elem_ptr115 = getelementptr i8, ptr %final_data113, i64 %offset114
  store i1 %43, ptr %raw_elem_ptr115, align 1
  %new_len116 = add i64 %curr_len104, 1
  store i64 %new_len116, ptr %len_ptr101, align 4
  %47 = getelementptr inbounds nuw %struct_name_age_hasJob_savings, ptr %row52, i32 0, i32 3
  %48 = load i64, ptr %47, align 4
  %col_ptr_ptr117 = getelementptr ptr, ptr %31, i64 3
  %49 = load ptr, ptr %col_ptr_ptr117, align 8
  %len_ptr118 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %49, i32 0, i32 0
  %cap_ptr119 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %49, i32 0, i32 1
  %data_ptr_ptr120 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %49, i32 0, i32 2
  %curr_len121 = load i64, ptr %len_ptr118, align 4
  %curr_cap122 = load i64, ptr %cap_ptr119, align 4
  %curr_data123 = load ptr, ptr %data_ptr_ptr120, align 8
  %needs_grow124 = icmp sge i64 %curr_len121, %curr_cap122
  br i1 %needs_grow124, label %grow125, label %store_element126

grow125:                                          ; preds = %store_element109
  %50 = icmp eq i64 %curr_cap122, 0
  %51 = mul i64 %curr_cap122, 2
  %new_cap127 = select i1 %50, i64 4, i64 %51
  %new_byte_count128 = mul i64 %new_cap127, 8
  %reallocated_data129 = call ptr @realloc(ptr %curr_data123, i64 %new_byte_count128)
  store i64 %new_cap127, ptr %cap_ptr119, align 4
  store ptr %reallocated_data129, ptr %data_ptr_ptr120, align 8
  br label %store_element126

store_element126:                                 ; preds = %grow125, %store_element109
  %final_data130 = load ptr, ptr %data_ptr_ptr120, align 8
  %offset131 = mul i64 %curr_len121, 8
  %raw_elem_ptr132 = getelementptr i8, ptr %final_data130, i64 %offset131
  store i64 %48, ptr %raw_elem_ptr132, align 4
  %new_len133 = add i64 %curr_len121, 1
  store i64 %new_len133, ptr %len_ptr118, align 4
  %52 = getelementptr inbounds nuw %dataframe, ptr %__result_7c35_load, i32 0, i32 3
  %53 = load i64, ptr %52, align 4
  %54 = add i64 %53, 1
  store i64 %54, ptr %52, align 4
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @ReadCsvInternal(ptr, ptr)

declare i32 @strcmp(ptr, ptr)

declare ptr @realloc(ptr, i64)
