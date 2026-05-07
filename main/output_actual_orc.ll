; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }
%struct_name_age = type { ptr, i64 }

@str = private unnamed_addr constant [5 x i8] c"name\00", align 1
@str.1 = private unnamed_addr constant [4 x i8] c"age\00", align 1
@__result = external global ptr
@__i = external global i64
@x = external global ptr

define ptr @main_9() {
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
  %df = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%dataframe, ptr null, i32 1) to i32))
  %15 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 0
  store ptr %arr_header4, ptr %15, align 8
  %16 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 1
  store ptr %data_ptrs_header, ptr %16, align 8
  %17 = getelementptr inbounds nuw %dataframe, ptr %df, i32 0, i32 2
  store ptr %arr_header7, ptr %17, align 8
  store ptr %df, ptr @__result, align 8
  store i64 0, ptr @__i, align 8
  store i64 0, ptr @__i, align 8
  br label %for.cond

for.cond:                                         ; preds = %for.step, %entry
  %__i_load = load i64, ptr @__i, align 4
  %x_load = load ptr, ptr @x, align 8
  %rows_ptr_field = getelementptr inbounds nuw { ptr, ptr, ptr }, ptr %x_load, i32 0, i32 1
  %rows_ptr = load ptr, ptr %rows_ptr_field, align 8
  %rows_array_ptr = bitcast ptr %rows_ptr to ptr
  %rows_length_ptr = getelementptr inbounds nuw { i64, i64, ptr }, ptr %rows_array_ptr, i32 0, i32 0
  %rows_length = load i64, ptr %rows_length_ptr, align 8
  %icmp_tmp = icmp slt i64 %__i_load, %rows_length
  br i1 %icmp_tmp, label %for.body, label %for.end

for.body:                                         ; preds = %for.cond
  %__i_load11 = load i64, ptr @__i, align 4
  %x_load12 = load ptr, ptr @x, align 8
  %data_ptrs_ptr = getelementptr inbounds nuw %dataframe, ptr %x_load12, i32 0, i32 1
  %data_ptrs_array = load ptr, ptr %data_ptrs_ptr, align 8
  %data_ptrs_data = getelementptr inbounds nuw %array, ptr %data_ptrs_array, i32 0, i32 2
  %data_ptrs = load ptr, ptr %data_ptrs_data, align 8
  %row = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr = getelementptr ptr, ptr %data_ptrs, i64 0
  %col_array = load ptr, ptr %col_ptr_ptr, align 8
  %col_data_ptr_ptr = getelementptr inbounds nuw %array, ptr %col_array, i32 0, i32 2
  %col_data_raw = load ptr, ptr %col_data_ptr_ptr, align 8
  %elem_ptr13 = getelementptr ptr, ptr %col_data_raw, i64 %__i_load11
  %val = load ptr, ptr %elem_ptr13, align 8
  %field_ptr = getelementptr inbounds nuw %struct_name_age, ptr %row, i32 0, i32 0
  store ptr %val, ptr %field_ptr, align 8
  %col_ptr_ptr14 = getelementptr ptr, ptr %data_ptrs, i64 1
  %col_array15 = load ptr, ptr %col_ptr_ptr14, align 8
  %col_data_ptr_ptr16 = getelementptr inbounds nuw %array, ptr %col_array15, i32 0, i32 2
  %col_data_raw17 = load ptr, ptr %col_data_ptr_ptr16, align 8
  %elem_ptr18 = getelementptr i64, ptr %col_data_raw17, i64 %__i_load11
  %val19 = load i64, ptr %elem_ptr18, align 4
  %field_ptr20 = getelementptr inbounds nuw %struct_name_age, ptr %row, i32 0, i32 1
  store i64 %val19, ptr %field_ptr20, align 4
  %__i_load21 = load i64, ptr @__i, align 4
  %x_load22 = load ptr, ptr @x, align 8
  %data_ptrs_ptr23 = getelementptr inbounds nuw %dataframe, ptr %x_load22, i32 0, i32 1
  %data_ptrs_array24 = load ptr, ptr %data_ptrs_ptr23, align 8
  %data_ptrs_data25 = getelementptr inbounds nuw %array, ptr %data_ptrs_array24, i32 0, i32 2
  %data_ptrs26 = load ptr, ptr %data_ptrs_data25, align 8
  %row27 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr28 = getelementptr ptr, ptr %data_ptrs26, i64 0
  %col_array29 = load ptr, ptr %col_ptr_ptr28, align 8
  %col_data_ptr_ptr30 = getelementptr inbounds nuw %array, ptr %col_array29, i32 0, i32 2
  %col_data_raw31 = load ptr, ptr %col_data_ptr_ptr30, align 8
  %elem_ptr32 = getelementptr ptr, ptr %col_data_raw31, i64 %__i_load21
  %val33 = load ptr, ptr %elem_ptr32, align 8
  %field_ptr34 = getelementptr inbounds nuw %struct_name_age, ptr %row27, i32 0, i32 0
  store ptr %val33, ptr %field_ptr34, align 8
  %col_ptr_ptr35 = getelementptr ptr, ptr %data_ptrs26, i64 1
  %col_array36 = load ptr, ptr %col_ptr_ptr35, align 8
  %col_data_ptr_ptr37 = getelementptr inbounds nuw %array, ptr %col_array36, i32 0, i32 2
  %col_data_raw38 = load ptr, ptr %col_data_ptr_ptr37, align 8
  %elem_ptr39 = getelementptr i64, ptr %col_data_raw38, i64 %__i_load21
  %val40 = load i64, ptr %elem_ptr39, align 4
  %field_ptr41 = getelementptr inbounds nuw %struct_name_age, ptr %row27, i32 0, i32 1
  store i64 %val40, ptr %field_ptr41, align 4
  %ptr_age = getelementptr inbounds nuw %struct_name_age, ptr %row27, i32 0, i32 1
  %val_age = load i64, ptr %ptr_age, align 4
  %icmp_tmp42 = icmp eq i64 %val_age, 30
  br i1 %icmp_tmp42, label %then, label %else

for.step:                                         ; preds = %ifcont
  %x_load95 = load i64, ptr @__i, align 8
  %inc_add = add i64 %x_load95, 1
  store i64 %inc_add, ptr @__i, align 8
  br label %for.cond

for.end:                                          ; preds = %for.cond
  %__result_load96 = load ptr, ptr @__result, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 7, ptr %tag_ptr, align 2
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr %__result_load96, ptr %data_ptr, align 8
  ret ptr %runtime_obj

then:                                             ; preds = %for.body
  %__i_load43 = load i64, ptr @__i, align 4
  %x_load44 = load ptr, ptr @x, align 8
  %data_ptrs_ptr45 = getelementptr inbounds nuw %dataframe, ptr %x_load44, i32 0, i32 1
  %data_ptrs_array46 = load ptr, ptr %data_ptrs_ptr45, align 8
  %data_ptrs_data47 = getelementptr inbounds nuw %array, ptr %data_ptrs_array46, i32 0, i32 2
  %data_ptrs48 = load ptr, ptr %data_ptrs_data47, align 8
  %row49 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr50 = getelementptr ptr, ptr %data_ptrs48, i64 0
  %col_array51 = load ptr, ptr %col_ptr_ptr50, align 8
  %col_data_ptr_ptr52 = getelementptr inbounds nuw %array, ptr %col_array51, i32 0, i32 2
  %col_data_raw53 = load ptr, ptr %col_data_ptr_ptr52, align 8
  %elem_ptr54 = getelementptr ptr, ptr %col_data_raw53, i64 %__i_load43
  %val55 = load ptr, ptr %elem_ptr54, align 8
  %field_ptr56 = getelementptr inbounds nuw %struct_name_age, ptr %row49, i32 0, i32 0
  store ptr %val55, ptr %field_ptr56, align 8
  %col_ptr_ptr57 = getelementptr ptr, ptr %data_ptrs48, i64 1
  %col_array58 = load ptr, ptr %col_ptr_ptr57, align 8
  %col_data_ptr_ptr59 = getelementptr inbounds nuw %array, ptr %col_array58, i32 0, i32 2
  %col_data_raw60 = load ptr, ptr %col_data_ptr_ptr59, align 8
  %elem_ptr61 = getelementptr i64, ptr %col_data_raw60, i64 %__i_load43
  %val62 = load i64, ptr %elem_ptr61, align 4
  %field_ptr63 = getelementptr inbounds nuw %struct_name_age, ptr %row49, i32 0, i32 1
  store i64 %val62, ptr %field_ptr63, align 4
  %__result_load = load ptr, ptr @__result, align 8
  %18 = bitcast ptr %__result_load to ptr
  %19 = getelementptr inbounds nuw %dataframe, ptr %18, i32 0, i32 1
  %20 = load ptr, ptr %19, align 8
  %21 = bitcast ptr %20 to ptr
  %22 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %21, i32 0, i32 0
  %23 = load i64, ptr %22, align 4
  %24 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %21, i32 0, i32 2
  %25 = load ptr, ptr %24, align 8
  %__i_load64 = load i64, ptr @__i, align 4
  %x_load65 = load ptr, ptr @x, align 8
  %data_ptrs_ptr66 = getelementptr inbounds nuw %dataframe, ptr %x_load65, i32 0, i32 1
  %data_ptrs_array67 = load ptr, ptr %data_ptrs_ptr66, align 8
  %data_ptrs_data68 = getelementptr inbounds nuw %array, ptr %data_ptrs_array67, i32 0, i32 2
  %data_ptrs69 = load ptr, ptr %data_ptrs_data68, align 8
  %row70 = tail call ptr @malloc(i32 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i32))
  %col_ptr_ptr71 = getelementptr ptr, ptr %data_ptrs69, i64 0
  %col_array72 = load ptr, ptr %col_ptr_ptr71, align 8
  %col_data_ptr_ptr73 = getelementptr inbounds nuw %array, ptr %col_array72, i32 0, i32 2
  %col_data_raw74 = load ptr, ptr %col_data_ptr_ptr73, align 8
  %elem_ptr75 = getelementptr ptr, ptr %col_data_raw74, i64 %__i_load64
  %val76 = load ptr, ptr %elem_ptr75, align 8
  %field_ptr77 = getelementptr inbounds nuw %struct_name_age, ptr %row70, i32 0, i32 0
  store ptr %val76, ptr %field_ptr77, align 8
  %col_ptr_ptr78 = getelementptr ptr, ptr %data_ptrs69, i64 1
  %col_array79 = load ptr, ptr %col_ptr_ptr78, align 8
  %col_data_ptr_ptr80 = getelementptr inbounds nuw %array, ptr %col_array79, i32 0, i32 2
  %col_data_raw81 = load ptr, ptr %col_data_ptr_ptr80, align 8
  %elem_ptr82 = getelementptr i64, ptr %col_data_raw81, i64 %__i_load64
  %val83 = load i64, ptr %elem_ptr82, align 4
  %field_ptr84 = getelementptr inbounds nuw %struct_name_age, ptr %row70, i32 0, i32 1
  store i64 %val83, ptr %field_ptr84, align 4
  %26 = getelementptr inbounds nuw %struct_name_age, ptr %row70, i32 0, i32 0
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

ifcont:                                           ; preds = %else, %store_element90
  %iftmp = phi ptr [ %18, %store_element90 ], [ 0.000000e+00, %else ]
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
  %37 = getelementptr inbounds nuw %struct_name_age, ptr %row70, i32 0, i32 1
  %38 = load i64, ptr %37, align 4
  %39 = getelementptr ptr, ptr %25, i64 1
  %40 = load ptr, ptr %39, align 8
  %41 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %40, i32 0, i32 0
  %42 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %40, i32 0, i32 1
  %43 = getelementptr inbounds nuw { i64, i64, ptr }, ptr %40, i32 0, i32 2
  %curr_len85 = load i64, ptr %41, align 4
  %curr_cap86 = load i64, ptr %42, align 4
  %curr_data87 = load ptr, ptr %43, align 8
  %needs_grow88 = icmp sge i64 %curr_len85, %curr_cap86
  br i1 %needs_grow88, label %grow89, label %store_element90

grow89:                                           ; preds = %store_element
  %44 = icmp eq i64 %curr_cap86, 0
  %45 = mul i64 %curr_cap86, 2
  %new_cap91 = select i1 %44, i64 4, i64 %45
  %46 = mul i64 %new_cap91, 8
  %reallocated_data92 = call ptr @realloc(ptr %curr_data87, i64 %46)
  store i64 %new_cap91, ptr %42, align 4
  store ptr %reallocated_data92, ptr %43, align 8
  br label %store_element90

store_element90:                                  ; preds = %grow89, %store_element
  %final_data93 = load ptr, ptr %43, align 8
  %elem_dest94 = getelementptr i64, ptr %final_data93, i64 %curr_len85
  store i64 %38, ptr %elem_dest94, align 4
  %47 = add i64 %curr_len85, 1
  store i64 %47, ptr %41, align 4
  %48 = add i64 %23, 1
  store i64 %48, ptr %22, align 4
  br label %ifcont
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)

declare ptr @realloc(ptr, i64)
