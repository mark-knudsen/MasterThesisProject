; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr, i64 }
%struct_name_age_isCool_savings = type { ptr, i64, i1, double }

@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@str.2 = private unnamed_addr constant [7 x i8] c"isCool\00", align 1
@str.3 = private unnamed_addr constant [8 x i8] c"savings\00", align 1
@__result_1c98 = global ptr null
@__i_1c98 = global i64 0
@df = external global ptr
@str.4 = private unnamed_addr constant [6 x i8] c"Harry\00", align 1

define ptr @main_2() {
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
  store ptr %df_instance, ptr @__result_1c98, align 8
  store i64 0, ptr @__i_1c98, align 8
  store i64 0, ptr @__i_1c98, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__i_1c98_load = load i64, ptr @__i_1c98, align 4
  %df_load = load ptr, ptr @df, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr, i64 }, ptr %df_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__i_1c98_load, %rows_length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__i_1c98_load21 = load i64, ptr @__i_1c98, align 4
  %df_load22 = load ptr, ptr @df, align 8
  %data_ptrs_ptr = getelementptr inbounds nuw %dataframe, ptr %df_load22, i32 0, i32 1
  %header_ptr = load ptr, ptr %data_ptrs_ptr, align 8
  %data_ptrs_data_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %header_ptr, i32 0, i32 2
  %data_ptrs_raw = load ptr, ptr %data_ptrs_data_ptr, align 8
  %row = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age_isCool_savings, ptr null, i32 1) to i32))
  %col_ptr_ptr = getelementptr ptr, ptr %data_ptrs_raw, i64 0
  %col_array_header = load ptr, ptr %col_ptr_ptr, align 8
  %col_data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header, i32 0, i32 2
  %col_data_raw = load ptr, ptr %col_data_ptr_ptr, align 8
  %elem_ptr23 = getelementptr ptr, ptr %col_data_raw, i64 %__i_1c98_load21
  %val = load ptr, ptr %elem_ptr23, align 8
  %field_ptr = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row, i32 0, i32 0
  store ptr %val, ptr %field_ptr, align 8
  %col_ptr_ptr24 = getelementptr ptr, ptr %data_ptrs_raw, i64 1
  %col_array_header25 = load ptr, ptr %col_ptr_ptr24, align 8
  %col_data_ptr_ptr26 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header25, i32 0, i32 2
  %col_data_raw27 = load ptr, ptr %col_data_ptr_ptr26, align 8
  %elem_ptr28 = getelementptr i64, ptr %col_data_raw27, i64 %__i_1c98_load21
  %val29 = load i64, ptr %elem_ptr28, align 4
  %field_ptr30 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row, i32 0, i32 1
  store i64 %val29, ptr %field_ptr30, align 4
  %col_ptr_ptr31 = getelementptr ptr, ptr %data_ptrs_raw, i64 2
  %col_array_header32 = load ptr, ptr %col_ptr_ptr31, align 8
  %col_data_ptr_ptr33 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header32, i32 0, i32 2
  %col_data_raw34 = load ptr, ptr %col_data_ptr_ptr33, align 8
  %elem_ptr35 = getelementptr i8, ptr %col_data_raw34, i64 %__i_1c98_load21
  %raw = load i8, ptr %elem_ptr35, align 1
  %bool = trunc i8 %raw to i1
  %field_ptr36 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row, i32 0, i32 2
  store i1 %bool, ptr %field_ptr36, align 1
  %col_ptr_ptr37 = getelementptr ptr, ptr %data_ptrs_raw, i64 3
  %col_array_header38 = load ptr, ptr %col_ptr_ptr37, align 8
  %col_data_ptr_ptr39 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header38, i32 0, i32 2
  %col_data_raw40 = load ptr, ptr %col_data_ptr_ptr39, align 8
  %elem_ptr41 = getelementptr double, ptr %col_data_raw40, i64 %__i_1c98_load21
  %val42 = load double, ptr %elem_ptr41, align 8
  %field_ptr43 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row, i32 0, i32 3
  store double %val42, ptr %field_ptr43, align 8
  %__i_1c98_load44 = load i64, ptr @__i_1c98, align 4
  %df_load45 = load ptr, ptr @df, align 8
  %data_ptrs_ptr46 = getelementptr inbounds nuw %dataframe, ptr %df_load45, i32 0, i32 1
  %header_ptr47 = load ptr, ptr %data_ptrs_ptr46, align 8
  %data_ptrs_data_ptr48 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %header_ptr47, i32 0, i32 2
  %data_ptrs_raw49 = load ptr, ptr %data_ptrs_data_ptr48, align 8
  %row50 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age_isCool_savings, ptr null, i32 1) to i32))
  %col_ptr_ptr51 = getelementptr ptr, ptr %data_ptrs_raw49, i64 0
  %col_array_header52 = load ptr, ptr %col_ptr_ptr51, align 8
  %col_data_ptr_ptr53 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header52, i32 0, i32 2
  %col_data_raw54 = load ptr, ptr %col_data_ptr_ptr53, align 8
  %elem_ptr55 = getelementptr ptr, ptr %col_data_raw54, i64 %__i_1c98_load44
  %val56 = load ptr, ptr %elem_ptr55, align 8
  %field_ptr57 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row50, i32 0, i32 0
  store ptr %val56, ptr %field_ptr57, align 8
  %col_ptr_ptr58 = getelementptr ptr, ptr %data_ptrs_raw49, i64 1
  %col_array_header59 = load ptr, ptr %col_ptr_ptr58, align 8
  %col_data_ptr_ptr60 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header59, i32 0, i32 2
  %col_data_raw61 = load ptr, ptr %col_data_ptr_ptr60, align 8
  %elem_ptr62 = getelementptr i64, ptr %col_data_raw61, i64 %__i_1c98_load44
  %val63 = load i64, ptr %elem_ptr62, align 4
  %field_ptr64 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row50, i32 0, i32 1
  store i64 %val63, ptr %field_ptr64, align 4
  %col_ptr_ptr65 = getelementptr ptr, ptr %data_ptrs_raw49, i64 2
  %col_array_header66 = load ptr, ptr %col_ptr_ptr65, align 8
  %col_data_ptr_ptr67 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header66, i32 0, i32 2
  %col_data_raw68 = load ptr, ptr %col_data_ptr_ptr67, align 8
  %elem_ptr69 = getelementptr i8, ptr %col_data_raw68, i64 %__i_1c98_load44
  %raw70 = load i8, ptr %elem_ptr69, align 1
  %bool71 = trunc i8 %raw70 to i1
  %field_ptr72 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row50, i32 0, i32 2
  store i1 %bool71, ptr %field_ptr72, align 1
  %col_ptr_ptr73 = getelementptr ptr, ptr %data_ptrs_raw49, i64 3
  %col_array_header74 = load ptr, ptr %col_ptr_ptr73, align 8
  %col_data_ptr_ptr75 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header74, i32 0, i32 2
  %col_data_raw76 = load ptr, ptr %col_data_ptr_ptr75, align 8
  %elem_ptr77 = getelementptr double, ptr %col_data_raw76, i64 %__i_1c98_load44
  %val78 = load double, ptr %elem_ptr77, align 8
  %field_ptr79 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row50, i32 0, i32 3
  store double %val78, ptr %field_ptr79, align 8
  %ptr_name = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row50, i32 0, i32 0
  %val_name = load ptr, ptr %ptr_name, align 8
  %strcmp_res = call i32 @strcmp(ptr %val_name, ptr @str.4)
  %str_eq = icmp eq i32 %strcmp_res, 0
  br i1 %str_eq, label %then, label %else

for.step:                                         ; preds = %ifcont
  %x_load = load i64, ptr @__i_1c98, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__i_1c98, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__result_1c98_load168 = load ptr, ptr @__result_1c98, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__result_1c98_load168, ptr %data_ptr, align 8
  ret ptr %runtime_obj

then:                                             ; preds = %for.body
  %__i_1c98_load80 = load i64, ptr @__i_1c98, align 4
  %df_load81 = load ptr, ptr @df, align 8
  %data_ptrs_ptr82 = getelementptr inbounds nuw %dataframe, ptr %df_load81, i32 0, i32 1
  %header_ptr83 = load ptr, ptr %data_ptrs_ptr82, align 8
  %data_ptrs_data_ptr84 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %header_ptr83, i32 0, i32 2
  %data_ptrs_raw85 = load ptr, ptr %data_ptrs_data_ptr84, align 8
  %row86 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age_isCool_savings, ptr null, i32 1) to i32))
  %col_ptr_ptr87 = getelementptr ptr, ptr %data_ptrs_raw85, i64 0
  %col_array_header88 = load ptr, ptr %col_ptr_ptr87, align 8
  %col_data_ptr_ptr89 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header88, i32 0, i32 2
  %col_data_raw90 = load ptr, ptr %col_data_ptr_ptr89, align 8
  %elem_ptr91 = getelementptr ptr, ptr %col_data_raw90, i64 %__i_1c98_load80
  %val92 = load ptr, ptr %elem_ptr91, align 8
  %field_ptr93 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row86, i32 0, i32 0
  store ptr %val92, ptr %field_ptr93, align 8
  %col_ptr_ptr94 = getelementptr ptr, ptr %data_ptrs_raw85, i64 1
  %col_array_header95 = load ptr, ptr %col_ptr_ptr94, align 8
  %col_data_ptr_ptr96 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header95, i32 0, i32 2
  %col_data_raw97 = load ptr, ptr %col_data_ptr_ptr96, align 8
  %elem_ptr98 = getelementptr i64, ptr %col_data_raw97, i64 %__i_1c98_load80
  %val99 = load i64, ptr %elem_ptr98, align 4
  %field_ptr100 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row86, i32 0, i32 1
  store i64 %val99, ptr %field_ptr100, align 4
  %col_ptr_ptr101 = getelementptr ptr, ptr %data_ptrs_raw85, i64 2
  %col_array_header102 = load ptr, ptr %col_ptr_ptr101, align 8
  %col_data_ptr_ptr103 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header102, i32 0, i32 2
  %col_data_raw104 = load ptr, ptr %col_data_ptr_ptr103, align 8
  %elem_ptr105 = getelementptr i8, ptr %col_data_raw104, i64 %__i_1c98_load80
  %raw106 = load i8, ptr %elem_ptr105, align 1
  %bool107 = trunc i8 %raw106 to i1
  %field_ptr108 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row86, i32 0, i32 2
  store i1 %bool107, ptr %field_ptr108, align 1
  %col_ptr_ptr109 = getelementptr ptr, ptr %data_ptrs_raw85, i64 3
  %col_array_header110 = load ptr, ptr %col_ptr_ptr109, align 8
  %col_data_ptr_ptr111 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %col_array_header110, i32 0, i32 2
  %col_data_raw112 = load ptr, ptr %col_data_ptr_ptr111, align 8
  %elem_ptr113 = getelementptr double, ptr %col_data_raw112, i64 %__i_1c98_load80
  %val114 = load double, ptr %elem_ptr113, align 8
  %field_ptr115 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row86, i32 0, i32 3
  store double %val114, ptr %field_ptr115, align 8
  %__result_1c98_load = load ptr, ptr @__result_1c98, align 8
  %df_cast = bitcast ptr %__result_1c98_load to ptr
  %25 = getelementptr inbounds nuw %dataframe, ptr %df_cast, i32 0, i32 1
  %26 = load ptr, ptr %25, align 8
  %data_array = bitcast ptr %26 to ptr
  %27 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_array, i32 0, i32 2
  %28 = load ptr, ptr %27, align 8
  %29 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row86, i32 0, i32 0
  %30 = load ptr, ptr %29, align 8
  %col_ptr_ptr116 = getelementptr ptr, ptr %28, i64 0
  %31 = load ptr, ptr %col_ptr_ptr116, align 8
  %len_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %31, i32 0, i32 0
  %cap_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %31, i32 0, i32 1
  %data_ptr_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %31, i32 0, i32 2
  %curr_len = load i64, ptr %len_ptr, align 4
  %curr_cap = load i64, ptr %cap_ptr, align 4
  %curr_data = load ptr, ptr %data_ptr_ptr, align 8
  %needs_grow = icmp sge i64 %curr_len, %curr_cap
  br i1 %needs_grow, label %grow, label %store_element

else:                                             ; preds = %for.body
  br label %ifcont

ifcont:                                           ; preds = %else, %store_element160
  %iftmp = phi ptr [ %df_cast, %store_element160 ], [ null, %else ]
  br label %for.step

grow:                                             ; preds = %then
  %32 = icmp eq i64 %curr_cap, 0
  %33 = mul i64 %curr_cap, 2
  %new_cap = select i1 %32, i64 4, i64 %33
  %new_byte_count = mul i64 %new_cap, 8
  %reallocated_data = call ptr @realloc(ptr %curr_data, i64 %new_byte_count)
  store i64 %new_cap, ptr %cap_ptr, align 4
  store ptr %reallocated_data, ptr %data_ptr_ptr, align 8
  br label %store_element

store_element:                                    ; preds = %grow, %then
  %final_data = load ptr, ptr %data_ptr_ptr, align 8
  %offset = mul i64 %curr_len, 8
  %raw_elem_ptr = getelementptr i8, ptr %final_data, i64 %offset
  store ptr %30, ptr %raw_elem_ptr, align 8
  %new_len = add i64 %curr_len, 1
  store i64 %new_len, ptr %len_ptr, align 4
  %34 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row86, i32 0, i32 1
  %35 = load i64, ptr %34, align 4
  %col_ptr_ptr117 = getelementptr ptr, ptr %28, i64 1
  %36 = load ptr, ptr %col_ptr_ptr117, align 8
  %len_ptr118 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %36, i32 0, i32 0
  %cap_ptr119 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %36, i32 0, i32 1
  %data_ptr_ptr120 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %36, i32 0, i32 2
  %curr_len121 = load i64, ptr %len_ptr118, align 4
  %curr_cap122 = load i64, ptr %cap_ptr119, align 4
  %curr_data123 = load ptr, ptr %data_ptr_ptr120, align 8
  %needs_grow124 = icmp sge i64 %curr_len121, %curr_cap122
  br i1 %needs_grow124, label %grow125, label %store_element126

grow125:                                          ; preds = %store_element
  %37 = icmp eq i64 %curr_cap122, 0
  %38 = mul i64 %curr_cap122, 2
  %new_cap127 = select i1 %37, i64 4, i64 %38
  %new_byte_count128 = mul i64 %new_cap127, 8
  %reallocated_data129 = call ptr @realloc(ptr %curr_data123, i64 %new_byte_count128)
  store i64 %new_cap127, ptr %cap_ptr119, align 4
  store ptr %reallocated_data129, ptr %data_ptr_ptr120, align 8
  br label %store_element126

store_element126:                                 ; preds = %grow125, %store_element
  %final_data130 = load ptr, ptr %data_ptr_ptr120, align 8
  %offset131 = mul i64 %curr_len121, 8
  %raw_elem_ptr132 = getelementptr i8, ptr %final_data130, i64 %offset131
  store i64 %35, ptr %raw_elem_ptr132, align 4
  %new_len133 = add i64 %curr_len121, 1
  store i64 %new_len133, ptr %len_ptr118, align 4
  %39 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row86, i32 0, i32 2
  %40 = load i1, ptr %39, align 1
  %col_ptr_ptr134 = getelementptr ptr, ptr %28, i64 2
  %41 = load ptr, ptr %col_ptr_ptr134, align 8
  %len_ptr135 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %41, i32 0, i32 0
  %cap_ptr136 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %41, i32 0, i32 1
  %data_ptr_ptr137 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %41, i32 0, i32 2
  %curr_len138 = load i64, ptr %len_ptr135, align 4
  %curr_cap139 = load i64, ptr %cap_ptr136, align 4
  %curr_data140 = load ptr, ptr %data_ptr_ptr137, align 8
  %needs_grow141 = icmp sge i64 %curr_len138, %curr_cap139
  br i1 %needs_grow141, label %grow142, label %store_element143

grow142:                                          ; preds = %store_element126
  %42 = icmp eq i64 %curr_cap139, 0
  %43 = mul i64 %curr_cap139, 2
  %new_cap144 = select i1 %42, i64 4, i64 %43
  %new_byte_count145 = mul i64 %new_cap144, 1
  %reallocated_data146 = call ptr @realloc(ptr %curr_data140, i64 %new_byte_count145)
  store i64 %new_cap144, ptr %cap_ptr136, align 4
  store ptr %reallocated_data146, ptr %data_ptr_ptr137, align 8
  br label %store_element143

store_element143:                                 ; preds = %grow142, %store_element126
  %final_data147 = load ptr, ptr %data_ptr_ptr137, align 8
  %offset148 = mul i64 %curr_len138, 1
  %raw_elem_ptr149 = getelementptr i8, ptr %final_data147, i64 %offset148
  store i1 %40, ptr %raw_elem_ptr149, align 1
  %new_len150 = add i64 %curr_len138, 1
  store i64 %new_len150, ptr %len_ptr135, align 4
  %44 = getelementptr inbounds nuw %struct_name_age_isCool_savings, ptr %row86, i32 0, i32 3
  %45 = load double, ptr %44, align 8
  %col_ptr_ptr151 = getelementptr ptr, ptr %28, i64 3
  %46 = load ptr, ptr %col_ptr_ptr151, align 8
  %len_ptr152 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %46, i32 0, i32 0
  %cap_ptr153 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %46, i32 0, i32 1
  %data_ptr_ptr154 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %46, i32 0, i32 2
  %curr_len155 = load i64, ptr %len_ptr152, align 4
  %curr_cap156 = load i64, ptr %cap_ptr153, align 4
  %curr_data157 = load ptr, ptr %data_ptr_ptr154, align 8
  %needs_grow158 = icmp sge i64 %curr_len155, %curr_cap156
  br i1 %needs_grow158, label %grow159, label %store_element160

grow159:                                          ; preds = %store_element143
  %47 = icmp eq i64 %curr_cap156, 0
  %48 = mul i64 %curr_cap156, 2
  %new_cap161 = select i1 %47, i64 4, i64 %48
  %new_byte_count162 = mul i64 %new_cap161, 8
  %reallocated_data163 = call ptr @realloc(ptr %curr_data157, i64 %new_byte_count162)
  store i64 %new_cap161, ptr %cap_ptr153, align 4
  store ptr %reallocated_data163, ptr %data_ptr_ptr154, align 8
  br label %store_element160

store_element160:                                 ; preds = %grow159, %store_element143
  %final_data164 = load ptr, ptr %data_ptr_ptr154, align 8
  %offset165 = mul i64 %curr_len155, 8
  %raw_elem_ptr166 = getelementptr i8, ptr %final_data164, i64 %offset165
  store double %45, ptr %raw_elem_ptr166, align 8
  %new_len167 = add i64 %curr_len155, 1
  store i64 %new_len167, ptr %len_ptr152, align 4
  %49 = getelementptr inbounds nuw %dataframe, ptr %df_cast, i32 0, i32 3
  %50 = load i64, ptr %49, align 4
  %51 = add i64 %50, 1
  store i64 %51, ptr %49, align 4
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare i32 @strcmp(ptr, ptr)

declare ptr @realloc(ptr, i64)
