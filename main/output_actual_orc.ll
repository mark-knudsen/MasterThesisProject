; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }
%struct_name_age = type { ptr, i64 }

@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@__result_2 = global ptr null
@__i_2 = global i64 0
@df = external global ptr
@str.2 = private unnamed_addr constant [6 x i8] c"harry\00", align 1

define ptr @main_7() {
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
  %data_ptrs_header = call ptr @malloc(i64 24)
  %data_ptrs_buffer = call ptr @malloc(i64 16)
  %6 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_ptrs_header, i32 0, i32 0
  store i64 2, ptr %6, align 4
  %7 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_ptrs_header, i32 0, i32 1
  store i64 2, ptr %7, align 4
  %8 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_ptrs_header, i32 0, i32 2
  store ptr %data_ptrs_buffer, ptr %8, align 8
  %data_gep = getelementptr ptr, ptr %data_ptrs_buffer, i64 0
  store ptr %arr_header, ptr %data_gep, align 8
  %data_gep3 = getelementptr ptr, ptr %data_ptrs_buffer, i64 1
  store ptr %arr_header1, ptr %data_gep3, align 8
  %arr_header4 = call ptr @malloc(i64 24)
  %arr_data5 = call ptr @malloc(i64 16)
  %9 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 0
  %10 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 1
  %11 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header4, i32 0, i32 2
  store i64 2, ptr %9, align 4
  store i64 2, ptr %10, align 4
  store ptr %arr_data5, ptr %11, align 8
  %elem_ptr = getelementptr ptr, ptr %arr_data5, i64 0
  store ptr @str, ptr %elem_ptr, align 8
  %elem_ptr6 = getelementptr ptr, ptr %arr_data5, i64 1
  store ptr @str.1, ptr %elem_ptr6, align 8
  %arr_header7 = call ptr @malloc(i64 24)
  %arr_data8 = call ptr @malloc(i64 16)
  %12 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 0
  %13 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 1
  %14 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %arr_header7, i32 0, i32 2
  store i64 2, ptr %12, align 4
  store i64 2, ptr %13, align 4
  store ptr %arr_data8, ptr %14, align 8
  %elem_ptr9 = getelementptr ptr, ptr %arr_data8, i64 0
  store ptr null, ptr %elem_ptr9, align 8
  %elem_ptr10 = getelementptr ptr, ptr %arr_data8, i64 1
  store ptr null, ptr %elem_ptr10, align 8
  %df_instance = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %15 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 0
  store ptr %arr_header4, ptr %15, align 8
  %16 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 1
  store ptr %data_ptrs_header, ptr %16, align 8
  %17 = getelementptr inbounds nuw %dataframe, ptr %df_instance, i32 0, i32 2
  store ptr %arr_header7, ptr %17, align 8
  store ptr %df_instance, ptr @__result_2, align 8
  store i64 0, ptr @__i_2, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__i_2_load = load i64, ptr @__i_2, align 4
  %df_load = load ptr, ptr @df, align 8
  %data_ptrs_ptr = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %df_load, i32 0, i32 1
  %data_ptrs_array = load ptr, ptr %data_ptrs_ptr, align 8
  %data_ptrs_cast = bitcast ptr %data_ptrs_array to ptr
  %data_ptrs_data_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %data_ptrs_cast, i32 0, i32 2
  %data_ptrs = load ptr, ptr %data_ptrs_data_ptr, align 8
  %first_col_ptr_ptr = getelementptr ptr, ptr %data_ptrs, i64 0
  %first_col_ptr = load ptr, ptr %first_col_ptr_ptr, align 8
  %first_col_array = bitcast ptr %first_col_ptr to ptr
  %first_col_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %first_col_array, i32 0, i32 0
  %dataframe_length = load i64, ptr %first_col_length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__i_2_load, %dataframe_length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__i_2_load11 = load i64, ptr @__i_2, align 4
  %df_load12 = load ptr, ptr @df, align 8
  %data_ptrs_ptr13 = getelementptr inbounds nuw %dataframe, ptr %df_load12, i32 0, i32 1
  %data_ptrs_array14 = load ptr, ptr %data_ptrs_ptr13, align 8
  %data_ptrs_data = getelementptr inbounds nuw %array, ptr %data_ptrs_array14, i32 0, i32 2
  %data_ptrs15 = load ptr, ptr %data_ptrs_data, align 8
  %row = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr = getelementptr ptr, ptr %data_ptrs15, i64 0
  %col_array = load ptr, ptr %col_ptr_ptr, align 8
  %col_data_ptr_ptr = getelementptr inbounds nuw %array, ptr %col_array, i32 0, i32 2
  %col_data_raw = load ptr, ptr %col_data_ptr_ptr, align 8
  %elem_ptr16 = getelementptr ptr, ptr %col_data_raw, i64 %__i_2_load11
  %val = load ptr, ptr %elem_ptr16, align 8
  %field_ptr = getelementptr inbounds nuw %struct_name_age, ptr %row, i32 0, i32 0
  store ptr %val, ptr %field_ptr, align 8
  %col_ptr_ptr17 = getelementptr ptr, ptr %data_ptrs15, i64 1
  %col_array18 = load ptr, ptr %col_ptr_ptr17, align 8
  %col_data_ptr_ptr19 = getelementptr inbounds nuw %array, ptr %col_array18, i32 0, i32 2
  %col_data_raw20 = load ptr, ptr %col_data_ptr_ptr19, align 8
  %elem_ptr21 = getelementptr i64, ptr %col_data_raw20, i64 %__i_2_load11
  %val22 = load i64, ptr %elem_ptr21, align 4
  %field_ptr23 = getelementptr inbounds nuw %struct_name_age, ptr %row, i32 0, i32 1
  store i64 %val22, ptr %field_ptr23, align 4
  %__i_2_load24 = load i64, ptr @__i_2, align 4
  %df_load25 = load ptr, ptr @df, align 8
  %data_ptrs_ptr26 = getelementptr inbounds nuw %dataframe, ptr %df_load25, i32 0, i32 1
  %data_ptrs_array27 = load ptr, ptr %data_ptrs_ptr26, align 8
  %data_ptrs_data28 = getelementptr inbounds nuw %array, ptr %data_ptrs_array27, i32 0, i32 2
  %data_ptrs29 = load ptr, ptr %data_ptrs_data28, align 8
  %row30 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr31 = getelementptr ptr, ptr %data_ptrs29, i64 0
  %col_array32 = load ptr, ptr %col_ptr_ptr31, align 8
  %col_data_ptr_ptr33 = getelementptr inbounds nuw %array, ptr %col_array32, i32 0, i32 2
  %col_data_raw34 = load ptr, ptr %col_data_ptr_ptr33, align 8
  %elem_ptr35 = getelementptr ptr, ptr %col_data_raw34, i64 %__i_2_load24
  %val36 = load ptr, ptr %elem_ptr35, align 8
  %field_ptr37 = getelementptr inbounds nuw %struct_name_age, ptr %row30, i32 0, i32 0
  store ptr %val36, ptr %field_ptr37, align 8
  %col_ptr_ptr38 = getelementptr ptr, ptr %data_ptrs29, i64 1
  %col_array39 = load ptr, ptr %col_ptr_ptr38, align 8
  %col_data_ptr_ptr40 = getelementptr inbounds nuw %array, ptr %col_array39, i32 0, i32 2
  %col_data_raw41 = load ptr, ptr %col_data_ptr_ptr40, align 8
  %elem_ptr42 = getelementptr i64, ptr %col_data_raw41, i64 %__i_2_load24
  %val43 = load i64, ptr %elem_ptr42, align 4
  %field_ptr44 = getelementptr inbounds nuw %struct_name_age, ptr %row30, i32 0, i32 1
  store i64 %val43, ptr %field_ptr44, align 4
  %ptr_name = getelementptr inbounds nuw %struct_name_age, ptr %row30, i32 0, i32 0
  %val_name = load ptr, ptr %ptr_name, align 8
  %strcmp_res = call i32 @strcmp(ptr %val_name, ptr @str.2)
  %str_eq = icmp eq i32 %strcmp_res, 0
  %__i_2_load45 = load i64, ptr @__i_2, align 4
  %df_load46 = load ptr, ptr @df, align 8
  %data_ptrs_ptr47 = getelementptr inbounds nuw %dataframe, ptr %df_load46, i32 0, i32 1
  %data_ptrs_array48 = load ptr, ptr %data_ptrs_ptr47, align 8
  %data_ptrs_data49 = getelementptr inbounds nuw %array, ptr %data_ptrs_array48, i32 0, i32 2
  %data_ptrs50 = load ptr, ptr %data_ptrs_data49, align 8
  %row51 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr52 = getelementptr ptr, ptr %data_ptrs50, i64 0
  %col_array53 = load ptr, ptr %col_ptr_ptr52, align 8
  %col_data_ptr_ptr54 = getelementptr inbounds nuw %array, ptr %col_array53, i32 0, i32 2
  %col_data_raw55 = load ptr, ptr %col_data_ptr_ptr54, align 8
  %elem_ptr56 = getelementptr ptr, ptr %col_data_raw55, i64 %__i_2_load45
  %val57 = load ptr, ptr %elem_ptr56, align 8
  %field_ptr58 = getelementptr inbounds nuw %struct_name_age, ptr %row51, i32 0, i32 0
  store ptr %val57, ptr %field_ptr58, align 8
  %col_ptr_ptr59 = getelementptr ptr, ptr %data_ptrs50, i64 1
  %col_array60 = load ptr, ptr %col_ptr_ptr59, align 8
  %col_data_ptr_ptr61 = getelementptr inbounds nuw %array, ptr %col_array60, i32 0, i32 2
  %col_data_raw62 = load ptr, ptr %col_data_ptr_ptr61, align 8
  %elem_ptr63 = getelementptr i64, ptr %col_data_raw62, i64 %__i_2_load45
  %val64 = load i64, ptr %elem_ptr63, align 4
  %field_ptr65 = getelementptr inbounds nuw %struct_name_age, ptr %row51, i32 0, i32 1
  store i64 %val64, ptr %field_ptr65, align 4
  %__i_2_load66 = load i64, ptr @__i_2, align 4
  %df_load67 = load ptr, ptr @df, align 8
  %data_ptrs_ptr68 = getelementptr inbounds nuw %dataframe, ptr %df_load67, i32 0, i32 1
  %data_ptrs_array69 = load ptr, ptr %data_ptrs_ptr68, align 8
  %data_ptrs_data70 = getelementptr inbounds nuw %array, ptr %data_ptrs_array69, i32 0, i32 2
  %data_ptrs71 = load ptr, ptr %data_ptrs_data70, align 8
  %row72 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr73 = getelementptr ptr, ptr %data_ptrs71, i64 0
  %col_array74 = load ptr, ptr %col_ptr_ptr73, align 8
  %col_data_ptr_ptr75 = getelementptr inbounds nuw %array, ptr %col_array74, i32 0, i32 2
  %col_data_raw76 = load ptr, ptr %col_data_ptr_ptr75, align 8
  %elem_ptr77 = getelementptr ptr, ptr %col_data_raw76, i64 %__i_2_load66
  %val78 = load ptr, ptr %elem_ptr77, align 8
  %field_ptr79 = getelementptr inbounds nuw %struct_name_age, ptr %row72, i32 0, i32 0
  store ptr %val78, ptr %field_ptr79, align 8
  %col_ptr_ptr80 = getelementptr ptr, ptr %data_ptrs71, i64 1
  %col_array81 = load ptr, ptr %col_ptr_ptr80, align 8
  %col_data_ptr_ptr82 = getelementptr inbounds nuw %array, ptr %col_array81, i32 0, i32 2
  %col_data_raw83 = load ptr, ptr %col_data_ptr_ptr82, align 8
  %elem_ptr84 = getelementptr i64, ptr %col_data_raw83, i64 %__i_2_load66
  %val85 = load i64, ptr %elem_ptr84, align 4
  %field_ptr86 = getelementptr inbounds nuw %struct_name_age, ptr %row72, i32 0, i32 1
  store i64 %val85, ptr %field_ptr86, align 4
  %ptr_age = getelementptr inbounds nuw %struct_name_age, ptr %row72, i32 0, i32 1
  %val_age = load i64, ptr %ptr_age, align 4
  %icmp_tmp87 = icmp slt i64 %val_age, 20
  %andtmp = and i1 %str_eq, %icmp_tmp87
  br i1 %andtmp, label %then, label %else

for.step:                                         ; preds = %ifcont
  %x_load = load i64, ptr @__i_2, align 8
  %inc_add = add i64 %x_load, 1
  store i64 %inc_add, ptr @__i_2, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__result_2_load140 = load ptr, ptr @__result_2, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__result_2_load140, ptr %data_ptr, align 8
  ret ptr %runtime_obj

then:                                             ; preds = %for.body
  %__i_2_load88 = load i64, ptr @__i_2, align 4
  %df_load89 = load ptr, ptr @df, align 8
  %data_ptrs_ptr90 = getelementptr inbounds nuw %dataframe, ptr %df_load89, i32 0, i32 1
  %data_ptrs_array91 = load ptr, ptr %data_ptrs_ptr90, align 8
  %data_ptrs_data92 = getelementptr inbounds nuw %array, ptr %data_ptrs_array91, i32 0, i32 2
  %data_ptrs93 = load ptr, ptr %data_ptrs_data92, align 8
  %row94 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr95 = getelementptr ptr, ptr %data_ptrs93, i64 0
  %col_array96 = load ptr, ptr %col_ptr_ptr95, align 8
  %col_data_ptr_ptr97 = getelementptr inbounds nuw %array, ptr %col_array96, i32 0, i32 2
  %col_data_raw98 = load ptr, ptr %col_data_ptr_ptr97, align 8
  %elem_ptr99 = getelementptr ptr, ptr %col_data_raw98, i64 %__i_2_load88
  %val100 = load ptr, ptr %elem_ptr99, align 8
  %field_ptr101 = getelementptr inbounds nuw %struct_name_age, ptr %row94, i32 0, i32 0
  store ptr %val100, ptr %field_ptr101, align 8
  %col_ptr_ptr102 = getelementptr ptr, ptr %data_ptrs93, i64 1
  %col_array103 = load ptr, ptr %col_ptr_ptr102, align 8
  %col_data_ptr_ptr104 = getelementptr inbounds nuw %array, ptr %col_array103, i32 0, i32 2
  %col_data_raw105 = load ptr, ptr %col_data_ptr_ptr104, align 8
  %elem_ptr106 = getelementptr i64, ptr %col_data_raw105, i64 %__i_2_load88
  %val107 = load i64, ptr %elem_ptr106, align 4
  %field_ptr108 = getelementptr inbounds nuw %struct_name_age, ptr %row94, i32 0, i32 1
  store i64 %val107, ptr %field_ptr108, align 4
  %__result_2_load = load ptr, ptr @__result_2, align 8
  %18 = bitcast ptr %__result_2_load to ptr
  %19 = getelementptr inbounds nuw %dataframe, ptr %18, i32 0, i32 1
  %20 = load ptr, ptr %19, align 8
  %21 = bitcast ptr %20 to ptr
  %22 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %21, i32 0, i32 0
  %23 = load i64, ptr %22, align 4
  %24 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %21, i32 0, i32 2
  %25 = load ptr, ptr %24, align 8
  %__i_2_load109 = load i64, ptr @__i_2, align 4
  %df_load110 = load ptr, ptr @df, align 8
  %data_ptrs_ptr111 = getelementptr inbounds nuw %dataframe, ptr %df_load110, i32 0, i32 1
  %data_ptrs_array112 = load ptr, ptr %data_ptrs_ptr111, align 8
  %data_ptrs_data113 = getelementptr inbounds nuw %array, ptr %data_ptrs_array112, i32 0, i32 2
  %data_ptrs114 = load ptr, ptr %data_ptrs_data113, align 8
  %row115 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr116 = getelementptr ptr, ptr %data_ptrs114, i64 0
  %col_array117 = load ptr, ptr %col_ptr_ptr116, align 8
  %col_data_ptr_ptr118 = getelementptr inbounds nuw %array, ptr %col_array117, i32 0, i32 2
  %col_data_raw119 = load ptr, ptr %col_data_ptr_ptr118, align 8
  %elem_ptr120 = getelementptr ptr, ptr %col_data_raw119, i64 %__i_2_load109
  %val121 = load ptr, ptr %elem_ptr120, align 8
  %field_ptr122 = getelementptr inbounds nuw %struct_name_age, ptr %row115, i32 0, i32 0
  store ptr %val121, ptr %field_ptr122, align 8
  %col_ptr_ptr123 = getelementptr ptr, ptr %data_ptrs114, i64 1
  %col_array124 = load ptr, ptr %col_ptr_ptr123, align 8
  %col_data_ptr_ptr125 = getelementptr inbounds nuw %array, ptr %col_array124, i32 0, i32 2
  %col_data_raw126 = load ptr, ptr %col_data_ptr_ptr125, align 8
  %elem_ptr127 = getelementptr i64, ptr %col_data_raw126, i64 %__i_2_load109
  %val128 = load i64, ptr %elem_ptr127, align 4
  %field_ptr129 = getelementptr inbounds nuw %struct_name_age, ptr %row115, i32 0, i32 1
  store i64 %val128, ptr %field_ptr129, align 4
  %26 = getelementptr inbounds nuw %struct_name_age, ptr %row115, i32 0, i32 0
  %27 = load ptr, ptr %26, align 8
  %28 = getelementptr ptr, ptr %25, i64 0
  %29 = load ptr, ptr %28, align 8
  %30 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %29, i32 0, i32 0
  %31 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %29, i32 0, i32 1
  %32 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %29, i32 0, i32 2
  %curr_len = load i64, ptr %30, align 4
  %curr_cap = load i64, ptr %31, align 4
  %curr_data = load ptr, ptr %32, align 8
  %needs_grow = icmp sge i64 %curr_len, %curr_cap
  br i1 %needs_grow, label %grow, label %store_element

else:                                             ; preds = %for.body
  br label %ifcont

ifcont:                                           ; preds = %else, %store_element135
  %iftmp = phi ptr [ %18, %store_element135 ], [ 0.000000e+00, %else ]
  br label %for.step

grow:                                             ; preds = %then
  %33 = icmp eq i64 %curr_cap, 0
  %34 = mul i64 %curr_cap, 2
  %new_cap = select i1 %33, i64 4, i64 %34
  %35 = mul i64 %new_cap, 8
  %reallocated_data = call ptr @realloc(ptr %curr_data, i64 %35)
  store i64 %new_cap, ptr %31, align 4
  store ptr %reallocated_data, ptr %32, align 8
  br label %store_element

store_element:                                    ; preds = %grow, %then
  %final_data = load ptr, ptr %32, align 8
  %elem_dest = getelementptr ptr, ptr %final_data, i64 %curr_len
  store ptr %27, ptr %elem_dest, align 8
  %36 = add i64 %curr_len, 1
  store i64 %36, ptr %30, align 4
  %37 = getelementptr inbounds nuw %struct_name_age, ptr %row115, i32 0, i32 1
  %38 = load i64, ptr %37, align 4
  %39 = getelementptr ptr, ptr %25, i64 1
  %40 = load ptr, ptr %39, align 8
  %41 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %40, i32 0, i32 0
  %42 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %40, i32 0, i32 1
  %43 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %40, i32 0, i32 2
  %curr_len130 = load i64, ptr %41, align 4
  %curr_cap131 = load i64, ptr %42, align 4
  %curr_data132 = load ptr, ptr %43, align 8
  %needs_grow133 = icmp sge i64 %curr_len130, %curr_cap131
  br i1 %needs_grow133, label %grow134, label %store_element135

grow134:                                          ; preds = %store_element
  %44 = icmp eq i64 %curr_cap131, 0
  %45 = mul i64 %curr_cap131, 2
  %new_cap136 = select i1 %44, i64 4, i64 %45
  %46 = mul i64 %new_cap136, 8
  %reallocated_data137 = call ptr @realloc(ptr %curr_data132, i64 %46)
  store i64 %new_cap136, ptr %42, align 4
  store ptr %reallocated_data137, ptr %43, align 8
  br label %store_element135

store_element135:                                 ; preds = %grow134, %store_element
  %final_data138 = load ptr, ptr %43, align 8
  %elem_dest139 = getelementptr i64, ptr %final_data138, i64 %curr_len130
  store i64 %38, ptr %elem_dest139, align 4
  %47 = add i64 %curr_len130, 1
  store i64 %47, ptr %41, align 4
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare i32 @strcmp(ptr, ptr)

declare ptr @realloc(ptr, i64)
